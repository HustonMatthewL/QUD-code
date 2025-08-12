using XRL.World;

namespace XRL
{
	public abstract class IEventBinder : ITokenized, IComposite
	{
		public bool WantFieldReflection => false;

		int ITokenized.Token { get; set; }

		public virtual void WriteBind(SerializationWriter Writer, IEventHandler Handler, int ID)
		{
		}

		public virtual IEventHandler ReadBind(SerializationReader Reader, int ID)
		{
			return null;
		}
	}
}
