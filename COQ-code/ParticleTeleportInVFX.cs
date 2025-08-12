using System;
using System.Collections.Generic;
using Kobold;
using UnityEngine;

[ExecuteAlways]
public class ParticleTeleportInVFX : MonoBehaviour, CombatJuice.ICombatJuiceConfigurable
{
	public GameObject backingSprite;

	public bool teleportingOut;

	public bool reset = true;

	public int count;

	public float delay;

	public float duration = 0.5f;

	public float delaycount;

	public Texture2D texture;

	public ParticleSystem system;

	private ParticleSystem.Particle[] gos = new ParticleSystem.Particle[384];

	private Dictionary<int, Vector3> startPositions = new Dictionary<int, Vector3>();

	private Dictionary<int, Vector3> endPositions = new Dictionary<int, Vector3>();

	private Dictionary<int, int> distances = new Dictionary<int, int>();

	private Dictionary<int, float> angles = new Dictionary<int, float>();

	private float t;

	public void configure(string configurationString)
	{
		texture = SpriteManager.GetUnitySprite(configurationString).texture;
		reset = true;
	}

	private void Awake()
	{
		reset = true;
	}

	private void Update()
	{
		if (reset)
		{
			if (backingSprite != null)
			{
				backingSprite.SetActive(value: true);
			}
			system.Stop();
			system.Clear();
			reset = false;
			count = 0;
			startPositions.Clear();
			endPositions.Clear();
			distances.Clear();
			angles.Clear();
			for (int i = 0; i < texture.width; i++)
			{
				for (int j = 0; j < texture.height; j++)
				{
					if (texture.GetPixel(i, j).a > 0f)
					{
						endPositions.Add(count, new Vector3(i, j) + new Vector3(-7.5f, -11.5f));
						startPositions.Add(count, endPositions[count] + new Vector3(UnityEngine.Random.Range(-24f, 24f), UnityEngine.Random.Range(-24, 24), 0f));
						if (teleportingOut)
						{
							distances.Add(count, UnityEngine.Random.Range(64, 128));
							angles.Add(count, UnityEngine.Random.Range(0f, 1.2f));
						}
						else
						{
							distances.Add(count, UnityEngine.Random.Range(12, 256));
							angles.Add(count, UnityEngine.Random.Range(0f, 6.28f));
						}
						count++;
					}
				}
			}
			system.Play();
			system.Emit(count);
			t = 0f;
			delaycount = delay;
		}
		if (!(t < duration))
		{
			return;
		}
		if (delaycount > 0f)
		{
			bool flag = false;
			if (delaycount == delay)
			{
				flag = true;
			}
			delaycount -= Time.deltaTime;
			if (!flag)
			{
				return;
			}
		}
		else
		{
			t += Time.deltaTime;
			if (backingSprite != null && (double)t >= 0.8 && backingSprite.activeInHierarchy)
			{
				backingSprite?.SetActive(value: false);
			}
		}
		system.GetParticles(gos);
		for (int k = 0; k < count; k++)
		{
			float num;
			float num2;
			if (teleportingOut)
			{
				num = Mathf.Lerp(0f, 0f - angles[k], t / duration);
				num2 = Mathf.Lerp(0f, -distances[k], t / duration);
			}
			else
			{
				num = Mathf.Lerp(angles[k], 0f, t / duration);
				num2 = Mathf.Lerp(distances[k], 0f, t / duration);
			}
			double num3 = (double)endPositions[k].x + (double)num2 * Math.Cos(num);
			double num4 = (double)endPositions[k].y + (double)num2 * Math.Sin(num);
			gos[k].position = new Vector3((float)num3, (float)num4, gos[k].position.z);
		}
		system.SetParticles(gos);
	}
}
