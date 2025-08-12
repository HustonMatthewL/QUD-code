using System.CodeDom.Compiler;
using Occult.Engine.CodeGeneration;

namespace XRL.World.AI
{
	[GenerateSerializationPartial]
	public class OpinionInscrutable : IOpinionSubject
	{
		[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
		public override bool WantFieldReflection => false;

		public override int BaseValue => -500;

		[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
		public override void Write(SerializationWriter Writer)
		{
			Writer.Write(Magnitude);
			Writer.WriteOptimized(Time);
		}

		[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
		public override void Read(SerializationReader Reader)
		{
			Magnitude = Reader.ReadSingle();
			Time = Reader.ReadOptimizedInt64();
		}

		public override string GetText(GameObject Actor)
		{
			return "Inscrutable impulse.";
		}
	}
}
