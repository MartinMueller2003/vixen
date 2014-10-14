using System;
using Vixen.Module.SequenceType;
using VixenModules.Sequence.Timed;
using VixenModules.SequenceType.LightOrama;

namespace VixenModules.Sequence.LightOrama
{
	public class LightOramaSequenceModuleDescriptor : SequenceTypeModuleDescriptorBase
	{
		// [mem] still need to get a legitimate GUID
		private readonly Guid _typeId = new Guid("{92BBD2CB-B750-437F-8A88-49864D56FFFF}");

		public override string FileExtension
		{
			get { return ".lms"; }
		}

		public override Guid TypeId
		{
			get { return _typeId; }
		}

		public override Type ModuleClass
		{
			get { return typeof (LightOramaSequenceTypeModule); }
		}

		public override Type ModuleDataClass
		{
			get { return typeof (TimedSequenceData); }
		}

		public override Type ModuleStaticDataClass
		{
			get { return typeof (LightOramaSequenceStaticData); }
		}

		public override string Author
		{
			get { return "Martin Mueller"; }
		}

		public override string TypeName
		{
			get { return "Light-O-Rama Sequence"; }
		}

		public override string Description
		{
			get { return "Import sequences from Light-O-Rama"; }
		}

		public override string Version
		{
			get { return "1.0"; }
		}

		public override int ClassVersion
		{
			get { return 3; }
		}

		public override bool CanCreateNew
		{
			// Override to prevent creation of new sequence
			get { return false; }
		}
	}
}