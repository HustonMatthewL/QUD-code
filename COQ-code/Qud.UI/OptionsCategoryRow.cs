namespace Qud.UI
{
	public class OptionsCategoryRow : OptionsDataRow<bool>
	{
		public bool categoryExpanded
		{
			get
			{
				return base.Value;
			}
			set
			{
				base.Value = value;
			}
		}
	}
}
