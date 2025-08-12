using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kobold;
using Qud.UI;
using QupKit;
using RedShadow.CommonDialogs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XRL.EditorFormats.Map;
using XRL.UI;
using XRL.World;

namespace Overlay.MapEditor
{
	public class MapEditorView : BaseView
	{
		public struct MapEditorAction
		{
			public string Action;

			public MapFileCell Cell;

			public MapFileObjectBlueprint Brush;

			public Rect Region;

			public object AdditionalData;

			public MapEditorAction(string Action, MapFileCell Cell, MapFileObjectBlueprint Object)
			{
				this.Action = Action;
				this.Cell = Cell;
				Brush = Object;
				Region = Rect.zero;
				AdditionalData = null;
			}

			public MapEditorAction(string Action, MapFileCell Cell, MapFileObjectBlueprint Object, Rect Region, object AdditionalData = null)
			{
				this.Action = Action;
				this.Cell = Cell;
				Brush = Object;
				this.Region = Region;
				this.AdditionalData = AdditionalData;
			}
		}

		private struct ReplaceOperationNotes
		{
			public MapFileObjectBlueprint Original;

			public MapFileObjectBlueprint Replacement;

			public Dictionary<MapFileCellReference, int> replaceCount;
		}

		public class UpdateComponent : MonoBehaviour
		{
			public void Update()
			{
				instance?.Update();
			}
		}

		public static MapEditorView instance;

		private MapFile Map;

		public string FileName;

		public Rect SelectedRegion = Rect.zero;

		public bool HasSelectedRegion;

		public MapFileRegion Brush = new MapFileRegion(1, 1);

		private bool _FilterUsed;

		public string _DataFilter = "";

		public Stack<MapEditorAction> UndoStack = new Stack<MapEditorAction>();

		public Stack<MapEditorAction> RedoStack = new Stack<MapEditorAction>();

		private float t;

		private bool _HasRendered;

		private bool _HasSelectionRendered;

		private Vector2 _dragStart = Vector2.zero;

		private Vector2 _dragDelta = Vector2.zero;

		private bool isDragging;

		private Vector3 _cameraStart;

		public MenuBar MenuBar;

		public Menu ContextMenu;

		public bool setup;

		public static MapEditorView Instance;

		public UnityEngine.GameObject SelectionFrame;

		public UnityEngine.GameObject GhostHighlight;

		private MapFileObjectBlueprint HoverBlueprint;

		private MapFileObjectBlueprint ContextBlueprint;

		public bool FilterUsed
		{
			get
			{
				return _FilterUsed;
			}
			set
			{
				_FilterUsed = value;
				FindChild("FilterUsedButton").GetComponent<Toggle>().isOn = value;
				UpdateFilter();
			}
		}

		public string DataFilter
		{
			get
			{
				return _DataFilter;
			}
			set
			{
				_DataFilter = value;
				UpdateFilter();
			}
		}

		public override void OnCreate()
		{
			instance = this;
			OnDrag = delegate(PointerEventData D)
			{
				OnDragMove(D.position, D.delta, D.button);
			};
			OnClicked = delegate(PointerEventData D)
			{
				OnClick(D.position, D.button);
			};
			if (!base.rootObject.TryGetComponent<UpdateComponent>(out var _))
			{
				base.rootObject.AddComponent<UpdateComponent>();
			}
		}

		public void LoadMap(string file)
		{
			UndoStack = new Stack<MapEditorAction>();
			RedoStack = new Stack<MapEditorAction>();
			HasSelectedRegion = false;
			Map = MapFile.LoadFile(file);
			Map.Cells.FillEmptyCells();
			FileName = file;
			GetChild("Title").rootObject.GetComponent<TextMeshProUGUI>().text = "Map Editor - " + FileName;
			RenderMap();
			RenderSelectionFrame();
			if (FilterUsed)
			{
				UpdateFilter();
			}
		}

		public bool IsSingleObjectBrush()
		{
			if (Brush.width == 1 && Brush.height == 1)
			{
				return Brush.GetOrCreateCellAt(0, 0).Objects.Count == 1;
			}
			return false;
		}

		public string GetBrushText()
		{
			if (Brush.width == 1 && Brush.height == 1)
			{
				MapFileCell orCreateCellAt = Brush.GetOrCreateCellAt(0, 0);
				if (orCreateCellAt.Objects.Count == 1)
				{
					return orCreateCellAt.Objects[0].Name;
				}
				List<MapFileObjectBlueprint> list = new List<MapFileObjectBlueprint>();
				foreach (MapFileObjectReference item in Brush.AllObjects())
				{
					list.Add(item.blueprint);
				}
				return list.Count + " objects";
			}
			List<MapFileObjectBlueprint> list2 = new List<MapFileObjectBlueprint>();
			foreach (MapFileObjectReference item2 in Brush.AllObjects())
			{
				list2.Add(item2.blueprint);
			}
			return $"{Brush.width}x{Brush.height} brush, {list2.Count} objects";
		}

		public void SetSelectedBlueprint(string blueprintName)
		{
			Brush = new MapFileRegion(1, 1);
			Brush.GetOrCreateCellAt(0, 0).Objects.Add(new MapFileObjectBlueprint(blueprintName));
			SetBrush(Brush);
		}

		public void SetSelectedBlueprint(MapFileObjectBlueprint blueprint)
		{
			Brush = new MapFileRegion(1, 1);
			Brush.GetOrCreateCellAt(0, 0).Objects.Add(new MapFileObjectBlueprint(blueprint));
			SetBrush(Brush);
		}

		public void SetBrush(MapFileRegion brush)
		{
			DataFilter = DataFilter;
			Brush = brush;
			GhostHighlight.SetActive(value: false);
			UnityEngine.UI.Text text = FindChild("SelectionHeader/SelectedItem")?.GetComponent<UnityEngine.UI.Text>();
			if ((bool)text)
			{
				text.text = $"Brush: {GetBrushText()}";
			}
			UpdateSelectedContents();
		}

		public void FilterUsedUpdated()
		{
			FilterUsed = FindChild("FilterUsedButton").GetComponent<Toggle>().isOn;
		}

		public void UpdateHoverXY(int X, int Y)
		{
			if (!IsSingleObjectBrush() && (Brush.width != 1 || Brush.height != 1))
			{
				GhostHighlight.transform.parent = GameManager.Instance.ConsoleCharacter[X, Y].transform;
				GhostHighlight.transform.localPosition = Vector3.back * 2f;
				GhostHighlight.GetComponent<MapEditorSelectionWidget>().SetTileSize(Brush.width, Brush.height);
				GhostHighlight.SetActive(value: true);
			}
			FindChild("SelectionHeader/PositionText").GetComponent<UnityEngine.UI.Text>().text = $"Mouse Position: {X}, {Y}";
		}

		public void UpdateSelectedXY()
		{
			if (!HasSelectedRegion)
			{
				FindChild("SelectionHeader/SelectedText").GetComponent<UnityEngine.UI.Text>().text = $"Selected Cell: none";
				return;
			}
			if (SelectedRegion.size == Vector2.zero)
			{
				FindChild("SelectionHeader/SelectedText").GetComponent<UnityEngine.UI.Text>().text = $"Selected Cell: {SelectedRegion.x}, {SelectedRegion.y}";
				return;
			}
			FindChild("SelectionHeader/SelectedText").GetComponent<UnityEngine.UI.Text>().text = $"Selected Cell: {SelectedRegion.xMin}, {SelectedRegion.yMin} - {SelectedRegion.xMax}, {SelectedRegion.yMax}";
		}

		public string GetDefaultPath()
		{
			return Path.Combine(Application.streamingAssetsPath, "Base").Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
		}

		private void UpdateFilter()
		{
			HashSet<string> secondaryFilter = null;
			if (FilterUsed)
			{
				secondaryFilter = Map.UsedBlueprints();
			}
			GetChild("BlueprintScrollerController").rootObject.GetComponent<BlueprintScrollerController>().UpdateDataFilter(DataFilter, secondaryFilter);
		}

		public override void Update()
		{
			t += Time.deltaTime;
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				HasSelectedRegion = false;
				RenderSelectionFrame();
			}
			if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				Debug.Log("undo");
				Undo();
			}
			if (Input.GetKeyDown(KeyCode.F1) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				ToggleOverlay("Backdrops/NorthShevaBackdrop");
			}
			if (Input.GetKeyDown(KeyCode.Y) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				Debug.Log("redo");
				Redo();
			}
			if (Input.GetKeyDown(KeyCode.N) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				NewButton();
			}
			if (Input.GetKeyDown(KeyCode.O) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				Load();
			}
			if (Input.GetKeyDown(KeyCode.S) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				Save();
			}
			if (Input.GetKeyDown(KeyCode.A) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				SelectAll();
			}
			if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
			{
				SetBrush(Map.Cells.GetRegion(SelectedRegion).Clone());
			}
		}

		public void SelectAll()
		{
			SelectedRegion = new Rect(0f, 0f, 79f, 24f);
			HasSelectedRegion = true;
			RenderSelectionFrame();
		}

		public void RenderSelectionFrame()
		{
			SelectionFrame.SetActive(HasSelectedRegion);
			if (HasSelectedRegion)
			{
				SelectionFrame.transform.parent = GameManager.Instance.ConsoleCharacter[(int)SelectedRegion.xMin, (int)SelectedRegion.yMin].transform;
				SelectionFrame.transform.localPosition = Vector3.back;
				SelectionFrame.GetComponent<MapEditorSelectionWidget>().SetTileSize((int)SelectedRegion.size.x + 1, (int)SelectedRegion.size.y + 1);
			}
			UpdateSelectedXY();
			UpdateSelectedContents();
		}

		public void OnClick(Vector2 FingerPos, PointerEventData.InputButton Button)
		{
			Vector3 vector = GameManager.MainCamera.GetComponent<Camera>().ScreenToWorldPoint(FingerPos);
			RaycastHit[] array = Physics.RaycastAll(new Vector3(vector.x, vector.y, -10000f), new Vector3(0f, 0f, 1f));
			foreach (RaycastHit raycastHit in array)
			{
				TileBehavior component = raycastHit.collider.gameObject.GetComponent<TileBehavior>();
				if (component != null)
				{
					if (Button == PointerEventData.InputButton.Left)
					{
						OnCommand("Tile:" + component.x + "," + component.y);
					}
					if (Button == PointerEventData.InputButton.Right)
					{
						OnCommand("RightTile:" + component.x + "," + component.y);
					}
					if (Button == PointerEventData.InputButton.Middle)
					{
						OnCommand("MiddleTile:" + component.x + "," + component.y);
					}
					return;
				}
			}
			HasSelectedRegion = false;
			RenderSelectionFrame();
			RenderMap();
		}

		private void UpdateSelectedContents()
		{
			if (!HasSelectedRegion)
			{
				GetChildComponent<SelectedCellContentsView>("SelectedCellContents").Set(new MapFileRegion(0, 0), allowReplace: false);
			}
			else
			{
				GetChildComponent<SelectedCellContentsView>("SelectedCellContents").Set(Map.Cells.GetRegion(SelectedRegion), IsSingleObjectBrush());
			}
		}

		public override void OnGUI()
		{
			if (!_HasRendered)
			{
				RenderMap();
				_HasRendered = true;
			}
			if (!_HasSelectionRendered)
			{
				RenderSelectionFrame();
				_HasSelectionRendered = true;
			}
		}

		public void OnBeginDrag(PointerEventData eventData, TileBehavior T)
		{
			if (T == null)
			{
				isDragging = false;
				HasSelectedRegion = false;
				return;
			}
			_cameraStart = Camera.main.transform.localPosition;
			_dragStart = new Vector2(T.x, T.y);
			_dragDelta = Vector2.zero;
			isDragging = true;
		}

		public bool OnDragMove(Vector2 FingerPos, Vector2 Delta, PointerEventData.InputButton Button)
		{
			_dragDelta += Delta;
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				Vector3 vector = GameManager.MainCamera.GetComponent<Camera>().ScreenToWorldPoint(FingerPos + Delta);
				RaycastHit[] array = Physics.RaycastAll(new Vector3(vector.x, vector.y, -10000f), new Vector3(0f, 0f, 1f));
				TileBehavior tileBehavior = null;
				RaycastHit[] array2 = array;
				foreach (RaycastHit raycastHit in array2)
				{
					TileBehavior component = raycastHit.collider.gameObject.GetComponent<TileBehavior>();
					if (component != null)
					{
						tileBehavior = component;
					}
				}
				if (tileBehavior != null && isDragging)
				{
					SelectedRegion = default(Rect);
					Vector2 rhs = new Vector2(tileBehavior.x, tileBehavior.y);
					SelectedRegion.min = Vector2.Min(_dragStart, rhs);
					SelectedRegion.max = Vector2.Max(_dragStart, rhs);
					HasSelectedRegion = true;
					RenderSelectionFrame();
				}
				return true;
			}
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				Vector3 vector2 = GameManager.MainCamera.GetComponent<Camera>().ScreenToWorldPoint(FingerPos + Delta);
				RaycastHit[] array3 = Physics.RaycastAll(new Vector3(vector2.x, vector2.y, -10000f), new Vector3(0f, 0f, 1f));
				bool flag = false;
				RaycastHit raycastHit2 = default(RaycastHit);
				RaycastHit[] array2 = array3;
				for (int i = 0; i < array2.Length; i++)
				{
					RaycastHit raycastHit3 = array2[i];
					if (raycastHit3.collider.gameObject.GetComponent<TileBehavior>() != null)
					{
						raycastHit2 = raycastHit3;
						flag = true;
					}
				}
				if (flag)
				{
					TileBehavior component2 = raycastHit2.collider.gameObject.GetComponent<TileBehavior>();
					if (Map != null && IsSingleObjectBrush())
					{
						int x = component2.x;
						int y = component2.y;
						string name = Brush.GetOrCreateCellAt(0, 0).Objects[0].Name;
						switch (Button)
						{
						case PointerEventData.InputButton.Left:
						{
							for (int j = 0; j < Map.Cells[x, y].Objects.Count; j++)
							{
								if (Map.Cells[x, y].Objects[j].Name == name)
								{
									return true;
								}
							}
							MapFileObjectBlueprint mapFileObjectBlueprint = new MapFileObjectBlueprint(name);
							Map.Cells[x, y].Objects.Add(mapFileObjectBlueprint);
							UndoStack.Push(new MapEditorAction("Add", Map.Cells[x, y], mapFileObjectBlueprint));
							RedoStack.Clear();
							RenderSelectionFrame();
							RenderMap();
							return true;
						}
						case PointerEventData.InputButton.Right:
						{
							if (Map.Cells[x, y].Objects.Count <= 0)
							{
								break;
							}
							Instance.RedoStack.Clear();
							List<string> list = new List<string>();
							foreach (MapFileObjectBlueprint @object in Map.Cells[x, y].Objects)
							{
								Instance.UndoStack.Push(new MapEditorAction("Remove", Map.Cells[x, y], @object));
								list.Add(@object.Name);
							}
							Map.Cells[x, y].Objects.Clear();
							RenderSelectionFrame();
							RenderMap();
							return true;
						}
						}
					}
				}
			}
			else
			{
				LetterboxCamera component3 = GameManager.MainCamera.GetComponent<LetterboxCamera>();
				Vector3 positionImmediately = _cameraStart - new Vector3(_dragDelta.x * component3.UnitsPerPixel, _dragDelta.y * component3.UnitsPerPixel, 0f);
				component3.SetPositionImmediately(positionImmediately);
			}
			return true;
		}

		public MapEditorAction ReverseAction(MapEditorAction A)
		{
			if (A.Action == "Add")
			{
				for (int i = 0; i < A.Cell.Objects.Count; i++)
				{
					if (A.Cell.Objects[i] == A.Brush)
					{
						A.Cell.Objects.RemoveAt(i);
						return new MapEditorAction("Remove", A.Cell, A.Brush);
					}
				}
			}
			if (A.Action == "AddMulti")
			{
				MapFileRegion mapFileRegion = (MapFileRegion)A.AdditionalData;
				int num = (int)A.Region.xMin;
				int num2 = (int)A.Region.yMin;
				foreach (MapFileObjectReference oref in mapFileRegion.AllObjects())
				{
					if (oref.x + num <= 79 && oref.y + num2 <= 24)
					{
						MapFileCell mapFileCell = Map.Cells[oref.x + num, oref.y + num2];
						int num3 = mapFileCell.Objects.FindLastIndex((MapFileObjectBlueprint o) => o.Name == oref.blueprint.Name);
						if (num3 != -1)
						{
							mapFileCell.Objects.RemoveAt(num3);
						}
					}
				}
				return new MapEditorAction("DeleteMulti", null, null, A.Region, A.AdditionalData);
			}
			if (A.Action == "DeleteMulti")
			{
				MapFileRegion mapFileRegion2 = (MapFileRegion)A.AdditionalData;
				int num4 = (int)A.Region.xMin;
				int num5 = (int)A.Region.yMin;
				foreach (MapFileObjectReference item in mapFileRegion2.AllObjects())
				{
					if (item.x + num4 <= 79 && item.y + num5 <= 24)
					{
						Map.Cells[item.x + num4, item.y + num5].Objects.Add(new MapFileObjectBlueprint(item.blueprint));
					}
				}
				return new MapEditorAction("AddMulti", null, null, A.Region, A.AdditionalData);
			}
			if (A.Action == "Remove")
			{
				A.Cell.Objects.Add(new MapFileObjectBlueprint(A.Brush));
				return new MapEditorAction("Add", A.Cell, A.Brush);
			}
			if (A.Action == "FlipH")
			{
				FlipHorizontal(A.Region);
			}
			if (A.Action == "FlipV")
			{
				FlipVertical(A.Region);
			}
			if (A.Action == "BulkDelete")
			{
				A.Action = "BulkAdd";
				MapFileRegion region = Map.Cells.GetRegion(A.Region);
				foreach (KeyValuePair<MapFileCellReference, int> item2 in A.AdditionalData as Dictionary<MapFileCellReference, int>)
				{
					for (int j = 0; j < item2.Value; j++)
					{
						region[item2.Key.x, item2.Key.y].Objects.Add(new MapFileObjectBlueprint(A.Brush));
					}
				}
			}
			else if (A.Action == "BulkAdd")
			{
				A.Action = "BulkDelete";
				MapFileRegion region2 = Map.Cells.GetRegion(A.Region);
				foreach (KeyValuePair<MapFileCellReference, int> item3 in A.AdditionalData as Dictionary<MapFileCellReference, int>)
				{
					int count = item3.Value;
					region2[item3.Key.x, item3.Key.y].Objects.RemoveAll((MapFileObjectBlueprint o) => o == A.Brush && count-- > 0);
				}
			}
			else if (A.Action == "BulkReplace")
			{
				MapFileRegion region3 = Map.Cells.GetRegion(A.Region);
				ReplaceOperationNotes notes = (ReplaceOperationNotes)A.AdditionalData;
				foreach (MapFileCellReference key in notes.replaceCount.Keys)
				{
					int toReplace = notes.replaceCount[key];
					for (int k = 0; k < toReplace; k++)
					{
						region3[key.x, key.y].Objects.Add(new MapFileObjectBlueprint(notes.Original));
					}
					region3[key.x, key.y].Objects.RemoveAll((MapFileObjectBlueprint o) => o == notes.Replacement && toReplace-- > 0);
				}
				MapFileObjectBlueprint original = notes.Original;
				notes.Original = notes.Replacement;
				notes.Replacement = original;
				A.AdditionalData = notes;
			}
			return A;
		}

		public override void OnCommand(string Command)
		{
			OnCommand(Command);
		}

		public override void OnLeave()
		{
			Options.OptionPlayScaleOverride = null;
			Camera.main.GetComponent<LetterboxCamera>().suspendUpdate = false;
			base.OnLeave();
			SelectionFrame?.SetActive(value: false);
			GhostHighlight?.SetActive(value: false);
		}

		public override void OnEnter()
		{
			Options.OptionPlayScaleOverride = "Fit";
			Camera.main.GetComponent<LetterboxCamera>().suspendUpdate = true;
			Instance = this;
			base.OnEnter();
			if (SelectionFrame == null)
			{
				SelectionFrame = PrefabManager.Create("UI/SelectionFrame");
			}
			if (GhostHighlight == null)
			{
				GhostHighlight = PrefabManager.Create("UI/SelectionFrame");
				GhostHighlight.GetComponent<MapEditorSelectionWidget>().Color1 = new Color(1f, 1f, 0f, 1f);
				GhostHighlight.GetComponent<MapEditorSelectionWidget>().Color2 = new Color(1f, 1f, 0f, 0.75f);
				GhostHighlight.SetActive(value: false);
			}
			SelectionFrame.GetComponent<MapEditorSelectionWidget>().SetTileSize(1, 1);
			SelectionFrame.SetActive(value: false);
			GetChild("BlueprintScrollerController").rootObject.GetComponent<BlueprintScrollerController>().UpdateDataFilter("", null);
			GameManager.Instance.PushGameView("MapEditor");
			_HasRendered = false;
			if (Map == null)
			{
				NewMap();
			}
			if (!setup)
			{
				setup = true;
				MenuBar = GetChild("MenuBar").rootObject.GetComponent<MenuBar>();
				Menu menu = DialogManager.createMenu();
				menu.addItem(null, "New map").setHotKeyText("Ctrl+N").setMenuItemClicked(NewButton);
				menu.addItem(null, "Load map...").setHotKeyText("Ctrl+O").setMenuItemClicked(Load);
				menu.addItem(null, "Save").setHotKeyText("Ctrl+S").setMenuItemClicked(Save);
				menu.addItem(null, "Save As...").setMenuItemClicked(SaveAs);
				menu.addSeparator();
				menu.addItem(null, "Reload Blueprints").setMenuItemClicked(ReloadBlueprints);
				menu.addSeparator();
				menu.addItem(null, "Exit").setMenuItemClicked(Exit);
				MenuBar.addMenu("File", menu);
				Menu menu2 = DialogManager.createMenu();
				menu2.addItem(null, "Select All").setHotKeyText("Ctrl+A").setMenuItemClicked(SelectAll);
				menu2.addItem(null, "Undo").setHotKeyText("Ctrl+Z").setMenuItemClicked(Undo);
				menu2.addItem(null, "Redo").setHotKeyText("Ctrl+Y").setMenuItemClicked(Redo);
				MenuBar.addMenu("Edit", menu2);
				Menu menu3 = DialogManager.createMenu();
				menu3.addItem(null, "Flip Horizontal").setMenuItemClicked(FlipHorizontal);
				menu3.addItem(null, "Flip Vertical").setMenuItemClicked(FlipVertical);
				MenuBar.addMenu("Transform", menu3);
				Menu menu4 = DialogManager.createMenu();
				menu4.addItem(null, "Toggle NorthSheva Overlay").setHotKeyText("Ctrl+F1").setMenuItemClicked(delegate
				{
					ToggleOverlay("Backdrops/NorthShevaBackdrop");
				});
				MenuBar.addMenu("View", menu4);
				ContextMenu = DialogManager.createMenu();
				ContextMenu.transform.localScale = new Vector3(0.5f, 0.5f);
				ContextMenu.addItem(null, "Set owner").setMenuItemClicked(SetOwnerForContext);
				ContextMenu.addItem(null, "Set part").setMenuItemClicked(SetPartForContext);
				ContextMenu.addItem(null, "Add string property").setMenuItemClicked(AddPropertyForContext);
				ContextMenu.addItem(null, "Add int property").setMenuItemClicked(AddIntPropertyForContext);
				ContextMenu.addItem(null, "Remove property").setMenuItemClicked(RemovePropertyForContext);
				ContextMenu.DestroyOnClose = false;
			}
		}

		public void FlipHorizontal()
		{
			Rect region = SelectedRegion;
			if (!HasSelectedRegion || (SelectedRegion.width == 0f && SelectedRegion.height == 0f))
			{
				region = new Rect(0f, 0f, 79f, 24f);
			}
			FlipHorizontal(region);
			UndoStack.Push(new MapEditorAction("FlipH", null, null, region));
			RedoStack.Clear();
		}

		public void FlipHorizontal(Rect region)
		{
			Map.Cells.SetRegion(Map.Cells.GetRegion(region).FlippedHorizontal(), (int)region.xMin, (int)region.yMin);
			Instance.RenderMap();
		}

		public void FlipVertical()
		{
			Rect region = SelectedRegion;
			if (!HasSelectedRegion || (SelectedRegion.width == 0f && SelectedRegion.height == 0f))
			{
				region = new Rect(0f, 0f, 79f, 24f);
			}
			FlipVertical(region);
			UndoStack.Push(new MapEditorAction("FlipV", null, null, region));
			RedoStack.Clear();
		}

		public void FlipVertical(Rect region)
		{
			Map.Cells.SetRegion(Map.Cells.GetRegion(region).FlippedVertical(), (int)region.xMin, (int)region.yMin);
			Instance.RenderMap();
		}

		public IEnumerator _ReloadBlueprints()
		{
			GetChild("Working")._rootObject.SetActive(value: true);
			GetChildComponent<UnityEngine.UI.Text>("Working/Text").text = "Reloading ObjectBlueprints.xml\nMay take a few seconds...";
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			try
			{
				GameObjectFactory.Factory.Hotload();
				GetChild("BlueprintScrollerController").rootObject.GetComponent<BlueprintScrollerController>().ResetCaches();
				UpdateFilter();
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Error hotloading", x);
				DialogManager.notify("There was an error while loading ObjectBlueprints.xml");
			}
			GetChild("Working")._rootObject.SetActive(value: false);
			GetChildComponent<UnityEngine.UI.Text>("Working/Text").text = "Working...";
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
		}

		public void ReloadBlueprints()
		{
			GameManager.Instance.StartCoroutine(_ReloadBlueprints());
		}

		public override bool BeforeLeave()
		{
			base.BeforeLeave();
			GameManager.Instance.PopGameView();
			return true;
		}

		public void Undo()
		{
			if (Map != null && UndoStack.Count > 0)
			{
				MapEditorAction a = UndoStack.Pop();
				MapEditorAction item = ReverseAction(a);
				RedoStack.Push(item);
			}
			RenderSelectionFrame();
			RenderMap();
		}

		public void Redo()
		{
			if (Map != null && RedoStack.Count > 0)
			{
				MapEditorAction a = RedoStack.Pop();
				MapEditorAction item = ReverseAction(a);
				UndoStack.Push(item);
			}
			RenderSelectionFrame();
			RenderMap();
		}

		public void NewMap()
		{
			Map = new MapFile();
			Map.Cells.FillEmptyCells();
			UndoStack = new Stack<MapEditorAction>();
			RedoStack = new Stack<MapEditorAction>();
			FileName = null;
			GetChild("Title").rootObject.GetComponent<TextMeshProUGUI>().text = "Map Editor - Untitled Map";
			_HasRendered = false;
			if (FilterUsed)
			{
				UpdateFilter();
			}
			RenderMap();
			RenderSelectionFrame();
		}

		public void NewButton()
		{
			DialogManager.createMessageDialog().show("Create a new map?", delegate(Buttons b)
			{
				if (b == Buttons.Yes)
				{
					NewMap();
				}
			}, null, Buttons.Yes | Buttons.No);
		}

		public void Exit()
		{
			DialogManager.createMessageDialog().show("Are you sure you want to go back and lose any unsaved changes?", delegate(Buttons b)
			{
				if (b == Buttons.Yes)
				{
					UIManager.showWindow("ModToolkit");
				}
			}, null, Buttons.Yes | Buttons.No);
		}

		public void Save()
		{
			if (FileName != null)
			{
				Map.Save(FileName);
				DialogManager.notify("Map saved!");
				return;
			}
			DialogManager.createFileDialog().show(FileDialogMode.Save, "Save Map", "*.rpm", delegate(string s)
			{
				if (s != null)
				{
					FileName = s;
					GetChild("Title").rootObject.GetComponent<TextMeshProUGUI>().text = "Map Editor - " + FileName;
					Map.Save(s);
					DialogManager.notify("Map saved!");
				}
			}, "newmap.rpm", GetDefaultPath());
		}

		public void SaveAs()
		{
			DialogManager.createFileDialog().show(FileDialogMode.Save, "Save Map As", "*.rpm", delegate(string s)
			{
				if (s != null)
				{
					FileName = s;
					GetChild("Title").rootObject.GetComponent<TextMeshProUGUI>().text = "Map Editor - " + FileName;
					Map.Save(s);
					DialogManager.notify("Map saved!");
				}
			}, Path.GetFileName(FileName).Replace(".", " - copy."), GetDefaultPath());
		}

		public void Load()
		{
			DialogManager.createFileDialog().show(FileDialogMode.Load, "Load Map", "*.rpm", delegate(string s)
			{
				if (s != null)
				{
					LoadMap(s);
				}
				DialogManager.notify("Map loaded!");
			}, null, GetDefaultPath());
		}

		public bool OnCommand(string Command, int nFinger = 0)
		{
			if (Command == "Undo")
			{
				Undo();
			}
			if (Command == "Redo")
			{
				Redo();
			}
			if (Command == "New")
			{
				NewButton();
			}
			if (Command == "Back")
			{
				Exit();
			}
			if (Command == "FilterUpdated")
			{
				DataFilter = base.rootObject.transform.Find("InputField").Find("Field").GetComponent<TMP_InputField>()
					.text;
				base.rootObject.transform.Find("InputField").Find("Field").GetComponent<TMP_InputField>()
					.Select();
			}
			if (Command == "SaveAs")
			{
				SaveAs();
			}
			if (Command == "Save")
			{
				Save();
			}
			if (Command == "Load")
			{
				Load();
			}
			if (Command.StartsWith("RightTile:"))
			{
				string obj = Command.Split(':')[1];
				int num = Convert.ToInt32(obj.Split(',')[0]);
				int num2 = Convert.ToInt32(obj.Split(',')[1]);
				if (IsSingleObjectBrush())
				{
					if (Map.Cells[num, num2].Objects.Count > 0)
					{
						UndoStack.Push(new MapEditorAction("Remove", Map.Cells[num, num2], Map.Cells[num, num2].Objects[0]));
						RedoStack.Clear();
						Map.Cells[num, num2].Objects.RemoveAt(0);
					}
				}
				else
				{
					Rect rect = new Rect(num, num2, Brush.width - 1, Brush.height - 1);
					MapFileRegion region = Map.Cells.GetRegion(rect);
					MapFileRegion mapFileRegion = region.Clone();
					foreach (MapFileCellReference item in new List<MapFileCellReference>(region.AllCells()))
					{
						item.cell.Objects.RemoveAll((MapFileObjectBlueprint t) => true);
					}
					UndoStack.Push(new MapEditorAction("DeleteMulti", null, null, rect, mapFileRegion));
					RedoStack.Clear();
				}
				RenderMap();
			}
			if (Command == "Fill" && IsSingleObjectBrush())
			{
				for (int i = 0; i < 80; i++)
				{
					for (int j = 0; j < 25; j++)
					{
						if (Map.Cells[i, j].Objects.Count == 0)
						{
							Map.Cells[i, j].Objects.Add(new MapFileObjectBlueprint(Brush.GetOrCreateCellAt(0, 0).Objects[0]));
						}
					}
				}
				RenderMap();
			}
			if (Command.StartsWith("Tile:"))
			{
				if (nFinger == 0)
				{
					if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
					{
						if (Map != null)
						{
							string obj2 = Command.Split(':')[1];
							int x = Convert.ToInt32(obj2.Split(',')[0]);
							int y = Convert.ToInt32(obj2.Split(',')[1]);
							if (Map.Cells[x, y].Objects.Count > 0)
							{
								SetSelectedBlueprint(Map.Cells[x, y].Objects[0]);
							}
						}
					}
					else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
					{
						string obj3 = Command.Split(':')[1];
						int num3 = Convert.ToInt32(obj3.Split(',')[0]);
						int num4 = Convert.ToInt32(obj3.Split(',')[1]);
						if (Map != null && IsSingleObjectBrush())
						{
							MapFileObjectBlueprint mapFileObjectBlueprint = new MapFileObjectBlueprint(Brush.GetOrCreateCellAt(0, 0).Objects[0]);
							Map.Cells[num3, num4].Objects.Add(mapFileObjectBlueprint);
							UndoStack.Push(new MapEditorAction("Add", Map.Cells[num3, num4], mapFileObjectBlueprint));
							RedoStack.Clear();
							RenderMap();
						}
						else
						{
							foreach (MapFileObjectReference item2 in Brush.AllObjects())
							{
								if (item2.x + num3 <= 79 && item2.y + num4 <= 24)
								{
									Map.Cells[item2.x + num3, item2.y + num4].Objects.Add(new MapFileObjectBlueprint(item2.blueprint));
								}
							}
							UndoStack.Push(new MapEditorAction("AddMulti", null, null, new Rect(num3, num4, Brush.width - 1, Brush.height - 1), Brush.Clone()));
							RedoStack.Clear();
							RenderMap();
						}
					}
					else if (Input.GetKey(KeyCode.LeftShift))
					{
						if (Map != null && IsSingleObjectBrush())
						{
							string obj4 = Command.Split(':')[1];
							int x2 = Convert.ToInt32(obj4.Split(',')[0]);
							int y2 = Convert.ToInt32(obj4.Split(',')[1]);
							MapFileObjectBlueprint mapFileObjectBlueprint2 = Brush.GetOrCreateCellAt(0, 0).Objects[0];
							MapFileRegion mapFileRegion2 = Flood(x2, y2, mapFileObjectBlueprint2, new MapFileRegion(80, 25));
							Dictionary<MapFileCellReference, int> dictionary = new Dictionary<MapFileCellReference, int>();
							foreach (MapFileCellReference item3 in mapFileRegion2.FindCellsWithObjectBlueprint(mapFileObjectBlueprint2))
							{
								dictionary.Add(item3, 1);
							}
							UndoStack.Push(new MapEditorAction("BulkAdd", null, mapFileObjectBlueprint2, new Rect(0f, 0f, 79f, 24f), dictionary));
						}
					}
					else if (Map != null)
					{
						string obj5 = Command.Split(':')[1];
						int num5 = Convert.ToInt32(obj5.Split(',')[0]);
						int num6 = Convert.ToInt32(obj5.Split(',')[1]);
						SelectedRegion = new Rect(num5, num6, 0f, 0f);
						HasSelectedRegion = true;
						RenderSelectionFrame();
					}
				}
				if (nFinger == 1)
				{
					string obj6 = Command.Split(':')[1];
					int x3 = Convert.ToInt32(obj6.Split(',')[0]);
					int y3 = Convert.ToInt32(obj6.Split(',')[1]);
					if (Map.Cells[x3, y3].Objects.Count > 0)
					{
						UndoStack.Push(new MapEditorAction("Remove", Map.Cells[x3, y3], Map.Cells[x3, y3].Objects[0]));
						RedoStack.Clear();
						Map.Cells[x3, y3].Objects.RemoveAt(0);
						RenderMap();
					}
				}
			}
			if (Command == "ToggleFilterUsed")
			{
				FilterUsed = !FilterUsed;
			}
			return true;
		}

		public void SetFilterUsed(bool value)
		{
			FilterUsed = value;
			UpdateFilter();
		}

		public void SetHoverBlueprint(MapFileObjectBlueprint HoverBlueprint)
		{
			this.HoverBlueprint = HoverBlueprint;
			_HasRendered = false;
		}

		public void SetPartForContext()
		{
			DialogManager.getString("New part:", ContextBlueprint.Part ?? "", delegate(string part)
			{
				if (string.IsNullOrEmpty(part))
				{
					part = null;
				}
				if (!HasSelectedRegion)
				{
					SelectedRegion = new Rect(0f, 0f, 79f, 24f);
				}
				List<MapFileCellReference> list = new List<MapFileCellReference>();
				list.AddRange(Map.Cells.GetRegion(SelectedRegion).FindCellsWithObjectBlueprint(ContextBlueprint));
				foreach (MapFileCellReference item in list)
				{
					foreach (MapFileObjectBlueprint @object in item.cell.Objects)
					{
						if (@object == ContextBlueprint)
						{
							@object.Part = part;
						}
					}
				}
				RenderMap();
				RenderSelectionFrame();
			});
		}

		public void AddPropertyForContext()
		{
			DialogManager.getPair("Define property name and value", "[Name]", "[Value]", delegate(string n, string v)
			{
				if (!n.IsNullOrEmpty() && !n.StartsWith("[") && !v.IsNullOrEmpty() && !v.StartsWith("["))
				{
					if (!HasSelectedRegion)
					{
						SelectedRegion = new Rect(0f, 0f, 79f, 24f);
					}
					List<MapFileCellReference> list = new List<MapFileCellReference>();
					list.AddRange(Map.Cells.GetRegion(SelectedRegion).FindCellsWithObjectBlueprint(ContextBlueprint));
					foreach (MapFileCellReference item in list)
					{
						foreach (MapFileObjectBlueprint @object in item.cell.Objects)
						{
							if (@object == ContextBlueprint)
							{
								MapFileObjectBlueprint mapFileObjectBlueprint = @object;
								if (mapFileObjectBlueprint.Properties == null)
								{
									mapFileObjectBlueprint.Properties = new Dictionary<string, string>();
								}
								@object.Properties[n] = v;
							}
						}
					}
					RenderMap();
					RenderSelectionFrame();
				}
			});
		}

		public void AddIntPropertyForContext()
		{
			DialogManager.getPair("Define property name and value", "[Name]", "1", delegate(string n, string v)
			{
				if (!n.IsNullOrEmpty() && !n.StartsWith("[") && !v.IsNullOrEmpty())
				{
					if (!int.TryParse(v, out var result))
					{
						DialogManager.error("Invalid property value. Value must be an integer.");
					}
					else
					{
						if (!HasSelectedRegion)
						{
							SelectedRegion = new Rect(0f, 0f, 79f, 24f);
						}
						List<MapFileCellReference> list = new List<MapFileCellReference>();
						list.AddRange(Map.Cells.GetRegion(SelectedRegion).FindCellsWithObjectBlueprint(ContextBlueprint));
						foreach (MapFileCellReference item in list)
						{
							foreach (MapFileObjectBlueprint @object in item.cell.Objects)
							{
								if (@object == ContextBlueprint)
								{
									MapFileObjectBlueprint mapFileObjectBlueprint = @object;
									if (mapFileObjectBlueprint.IntProperties == null)
									{
										mapFileObjectBlueprint.IntProperties = new Dictionary<string, int>();
									}
									@object.IntProperties[n] = result;
								}
							}
						}
						RenderMap();
						RenderSelectionFrame();
					}
				}
			});
		}

		public void RemovePropertyForContext()
		{
			Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
			StringBuilder stringBuilder = new StringBuilder();
			if (!HasSelectedRegion)
			{
				SelectedRegion = new Rect(0f, 0f, 79f, 24f);
			}
			List<MapFileCellReference> cells = new List<MapFileCellReference>();
			cells.AddRange(Map.Cells.GetRegion(SelectedRegion).FindCellsWithObjectBlueprint(ContextBlueprint));
			foreach (MapFileCellReference item in cells)
			{
				foreach (MapFileObjectBlueprint @object in item.cell.Objects)
				{
					if (@object != ContextBlueprint)
					{
						continue;
					}
					if (!@object.Properties.IsNullOrEmpty())
					{
						foreach (KeyValuePair<string, string> property in @object.Properties)
						{
							if (!dictionary.TryGetValue(property.Key, out var value))
							{
								value = (dictionary[property.Key] = new List<string>(1));
							}
							stringBuilder.Clear().Append('"').Append(property.Value)
								.Append('"');
							if (!value.Any(stringBuilder.ValueEquals))
							{
								value.Add(stringBuilder.ToString());
							}
						}
					}
					if (@object.IntProperties.IsNullOrEmpty())
					{
						continue;
					}
					foreach (KeyValuePair<string, int> intProperty in @object.IntProperties)
					{
						if (!dictionary.TryGetValue(intProperty.Key, out var value2))
						{
							value2 = (dictionary[intProperty.Key] = new List<string>(1));
						}
						stringBuilder.Clear().Append(intProperty.Value);
						if (!value2.Any(stringBuilder.ValueEquals))
						{
							value2.Add(stringBuilder.ToString());
						}
					}
				}
			}
			if (dictionary.IsNullOrEmpty())
			{
				DialogManager.info("No properties defined.");
				return;
			}
			List<string> keys = new List<string>();
			List<string> list3 = new List<string>();
			foreach (KeyValuePair<string, List<string>> item2 in dictionary)
			{
				keys.Add(item2.Key);
				list3.Add(stringBuilder.Clear().Append(item2.Key).Append(" (")
					.AppendJoin(", ", item2.Value)
					.Append(')')
					.ToString());
			}
			DialogManager.getChoice("Choose a property to remove", -1, list3, delegate(int i)
			{
				if (i != -1)
				{
					string key = keys[i];
					foreach (MapFileCellReference item3 in cells)
					{
						foreach (MapFileObjectBlueprint object2 in item3.cell.Objects)
						{
							if (!(object2 != ContextBlueprint))
							{
								object2.Properties?.Remove(key);
								object2.IntProperties?.Remove(key);
							}
						}
					}
					RenderMap();
					RenderSelectionFrame();
				}
			});
		}

		public void ToggleOverlay(string resource)
		{
		}

		public void SetOwnerForContext()
		{
			SetOwnerInRegion(ContextBlueprint);
		}

		public void SetOwnerInRegion(MapFileObjectBlueprint blueprint)
		{
			DialogManager.getString("New owner:", blueprint.Owner ?? "", delegate(string newOwner)
			{
				if (string.IsNullOrEmpty(newOwner))
				{
					newOwner = null;
				}
				if (!HasSelectedRegion)
				{
					SelectedRegion = new Rect(0f, 0f, 79f, 24f);
				}
				List<MapFileCellReference> list = new List<MapFileCellReference>();
				list.AddRange(Map.Cells.GetRegion(SelectedRegion).FindCellsWithObjectBlueprint(blueprint));
				foreach (MapFileCellReference item in list)
				{
					foreach (MapFileObjectBlueprint @object in item.cell.Objects)
					{
						if (@object == blueprint)
						{
							@object.Owner = newOwner;
						}
					}
				}
				RenderMap();
				RenderSelectionFrame();
			});
		}

		public void DeleteInRegion(MapFileObjectBlueprint blueprint)
		{
			if (!HasSelectedRegion)
			{
				SelectedRegion = new Rect(0f, 0f, 79f, 24f);
			}
			List<MapFileCellReference> list = new List<MapFileCellReference>();
			list.AddRange(Map.Cells.GetRegion(SelectedRegion).FindCellsWithObjectBlueprint(blueprint));
			bool deleteAll = SelectedRegion.width != 0f || SelectedRegion.height != 0f;
			int deleteCount = 0;
			Dictionary<MapFileCellReference, int> dictionary = new Dictionary<MapFileCellReference, int>();
			foreach (MapFileCellReference item in list)
			{
				dictionary.Add(item, item.cell.Objects.RemoveAll((MapFileObjectBlueprint obj) => obj == blueprint && (deleteAll || deleteCount++ == 0)));
			}
			RedoStack.Clear();
			UndoStack.Push(new MapEditorAction("BulkDelete", null, blueprint, SelectedRegion, dictionary));
			RenderMap();
			RenderSelectionFrame();
		}

		public void DisplayContextInRegion(MapFileObjectBlueprint blueprint)
		{
			ContextBlueprint = blueprint;
			ContextMenu.show(Input.mousePosition);
		}

		public void ReplaceInRegion(MapFileObjectBlueprint blueprint)
		{
			if (!IsSingleObjectBrush())
			{
				return;
			}
			if (!HasSelectedRegion)
			{
				SelectedRegion = new Rect(0f, 0f, 79f, 24f);
			}
			List<MapFileCellReference> list = new List<MapFileCellReference>();
			list.AddRange(Map.Cells.GetRegion(SelectedRegion).FindCellsWithObjectBlueprint(blueprint));
			ReplaceOperationNotes replaceOperationNotes = new ReplaceOperationNotes
			{
				Original = blueprint,
				Replacement = new MapFileObjectBlueprint(Brush.GetOrCreateCellAt(0, 0).Objects[0]),
				replaceCount = new Dictionary<MapFileCellReference, int>()
			};
			foreach (MapFileCellReference item in list)
			{
				int num = item.cell.Objects.RemoveAll((MapFileObjectBlueprint obj) => obj == blueprint);
				replaceOperationNotes.replaceCount.Add(item, num);
				for (int i = 0; i < num; i++)
				{
					item.cell.Objects.Add(new MapFileObjectBlueprint(replaceOperationNotes.Replacement));
				}
			}
			RedoStack.Clear();
			UndoStack.Push(new MapEditorAction("BulkReplace", null, blueprint, SelectedRegion, replaceOperationNotes));
			RenderSelectionFrame();
			RenderMap();
		}

		public MapFileRegion Flood(int x, int y, MapFileObjectBlueprint SelectedBlueprint, MapFileRegion workingRegion)
		{
			if (x < 0)
			{
				return workingRegion;
			}
			if (y < 0)
			{
				return workingRegion;
			}
			if (x >= 80)
			{
				return workingRegion;
			}
			if (y >= 25)
			{
				return workingRegion;
			}
			if (Map.Cells[x, y].Objects.Count > 0)
			{
				return workingRegion;
			}
			workingRegion.GetOrCreateCellAt(x, y).Objects.Add(new MapFileObjectBlueprint(SelectedBlueprint));
			Map.Cells[x, y].Objects.Add(new MapFileObjectBlueprint(SelectedBlueprint));
			Flood(x - 1, y, SelectedBlueprint, workingRegion);
			Flood(x + 1, y, SelectedBlueprint, workingRegion);
			Flood(x, y - 1, SelectedBlueprint, workingRegion);
			Flood(x, y + 1, SelectedBlueprint, workingRegion);
			_HasRendered = false;
			_HasSelectionRendered = false;
			return workingRegion;
		}

		public bool InsideSelection(int x, int y)
		{
			if (HasSelectedRegion && SelectedRegion.xMin <= (float)x && SelectedRegion.xMax >= (float)x && SelectedRegion.yMin <= (float)y)
			{
				return SelectedRegion.yMax >= (float)y;
			}
			return false;
		}

		public void RenderMap()
		{
			ex3DSprite2[,] consoleCharacter = GameManager.Instance.ConsoleCharacter;
			for (int i = 0; i < 80; i++)
			{
				for (int j = 0; j < 25; j++)
				{
					MapFileCellRender mapFileCellRender = new MapFileCellRender();
					Map.Cells[i, j].Render(mapFileCellRender, new MapFileCellReference(Map.Cells, i, j, null));
					if (InsideSelection(i, j) && Map.Cells[i, j].Objects.Contains(HoverBlueprint))
					{
						mapFileCellRender.Detail = new Color(0.25f, 0.25f, 0f, 1f);
						mapFileCellRender.Background = new Color(1f, 1f, 0f, 1f);
						mapFileCellRender.Foreground = new Color(0.5f, 0.5f, 0f, 1f);
					}
					if (mapFileCellRender.Char == '\0')
					{
						exTextureInfo textureInfo = SpriteManager.GetTextureInfo(mapFileCellRender.Tile);
						Color foreground = mapFileCellRender.Foreground;
						Color background = mapFileCellRender.Background;
						Color detail = mapFileCellRender.Detail;
						if (consoleCharacter[i, j].textureInfo != textureInfo || consoleCharacter[i, j].color != foreground || consoleCharacter[i, j].backcolor != background || consoleCharacter[i, j].detailcolor != detail)
						{
							consoleCharacter[i, j].textureInfo = textureInfo;
							consoleCharacter[i, j].color = foreground;
							consoleCharacter[i, j].detailcolor = detail;
							consoleCharacter[i, j].backcolor = background;
						}
					}
					else
					{
						exTextureInfo exTextureInfo = GameManager.Instance.CharInfos[(uint)mapFileCellRender.Char];
						Color foreground2 = mapFileCellRender.Foreground;
						Color background2 = mapFileCellRender.Background;
						if (consoleCharacter[i, j].textureInfo != exTextureInfo || consoleCharacter[i, j].backcolor != foreground2 || consoleCharacter[i, j].color != background2)
						{
							consoleCharacter[i, j].textureInfo = exTextureInfo;
							consoleCharacter[i, j].backcolor = foreground2;
							consoleCharacter[i, j].detailcolor = foreground2;
							consoleCharacter[i, j].color = background2;
						}
					}
				}
			}
		}
	}
}
