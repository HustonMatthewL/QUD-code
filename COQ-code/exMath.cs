using UnityEngine;

public static class exMath
{
	public static float Wrap(float _value, float _length, WrapMode _wrapMode)
	{
		float num = Mathf.Abs(_value);
		switch (_wrapMode)
		{
		case WrapMode.Loop:
			num %= _length;
			break;
		case WrapMode.PingPong:
		{
			int num2 = (int)(num / _length);
			num %= _length;
			if (num2 % 2 == 1)
			{
				num = _length - num;
			}
			break;
		}
		default:
			num = Mathf.Clamp(num, 0f, _length);
			break;
		}
		return num;
	}

	public static int Wrap(int _value, int _maxValue, WrapMode _wrapMode)
	{
		if (_maxValue == 0)
		{
			return 0;
		}
		if (_value < 0)
		{
			_value = -_value;
		}
		switch (_wrapMode)
		{
		case WrapMode.Loop:
			return _value % (_maxValue + 1);
		case WrapMode.PingPong:
		{
			int num = _value / _maxValue;
			_value %= _maxValue;
			if ((num & 1) == 1)
			{
				return _maxValue - _value;
			}
			break;
		}
		default:
			if (_value < 0)
			{
				return 0;
			}
			if (_value > _maxValue)
			{
				return _maxValue;
			}
			break;
		}
		return _value;
	}

	public static float SpringLerp(float _strength, float _deltaTime)
	{
		if (_deltaTime > 1f)
		{
			_deltaTime = 1f;
		}
		int num = Mathf.RoundToInt(_deltaTime * 1000f);
		_deltaTime = 0.001f * _strength;
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			num2 = Mathf.Lerp(num2, 1f, _deltaTime);
		}
		return num2;
	}

	public static float SpringLerp(float _from, float _to, float _strength, float _deltaTime)
	{
		if (_deltaTime > 1f)
		{
			_deltaTime = 1f;
		}
		int num = Mathf.RoundToInt(_deltaTime * 1000f);
		_deltaTime = 0.001f * _strength;
		float num2 = _from;
		for (int i = 0; i < num; i++)
		{
			num2 = Mathf.Lerp(num2, _to, _deltaTime);
		}
		return num2;
	}

	public static Vector2 SpringLerp(Vector2 _from, Vector2 _to, float _strength, float _deltaTime)
	{
		return Vector2.Lerp(_from, _to, SpringLerp(_strength, _deltaTime));
	}

	public static Vector3 SpringLerp(Vector3 _from, Vector3 _to, float _strength, float _deltaTime)
	{
		return Vector3.Lerp(_from, _to, SpringLerp(_strength, _deltaTime));
	}

	public static float Lerp(float _from, float _to, float _v)
	{
		return _from * (1f - _v) + _to * _v;
	}

	public static Vector2 Lerp(Vector2 _from, Vector2 _to, float _v)
	{
		return _from * (1f - _v) + _to * _v;
	}

	public static Vector3 Lerp(Vector3 _from, Vector3 _to, float _v)
	{
		return _from * (1f - _v) + _to * _v;
	}

	public static Color Lerp(Color _from, Color _to, float _v)
	{
		return new Color(_from.r + (_to.r - _from.r) * _v, _from.g + (_to.g - _from.g) * _v, _from.b + (_to.b - _from.b) * _v, _from.a + (_to.a - _from.a) * _v);
	}

	public static Rect Lerp(Rect _from, Rect _to, float _v)
	{
		Rect result = default(Rect);
		result.x = _from.x * (1f - _v) + _to.x * _v;
		result.y = _from.y * (1f - _v) + _to.y * _v;
		result.width = _from.width * (1f - _v) + _to.width * _v;
		result.height = _from.height * (1f - _v) + _to.height * _v;
		return result;
	}
}
