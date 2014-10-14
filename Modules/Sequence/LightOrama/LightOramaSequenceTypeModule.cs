using Vixen.Cache.Sequence;
using Vixen.Execution;
using Vixen.IO;
using Vixen.Module.SequenceType;
using Vixen.Sys;
using VixenModules.Sequence.Timed;
using VixenModules.SequenceType.LightOrama;
using System;
using System.Collections.Generic;
using Vixen.Module;
using Vixen.Services;
using Vixen.Module.App;

namespace VixenModules.Sequence.LightOrama
{
	public class LightOramaSequenceTypeModule : SequenceTypeModuleInstanceBase
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();
		private LightOramaSequenceStaticData _mappingData;

		public Dictionary<string, List<LorChannelMapping>> LightOramaMappings
		{
			get { return _mappingData.LightOramaMappings; }
		}

		public override IModuleDataModel StaticModuleData
		{
			get { return _mappingData; }
			set { _mappingData = value as LightOramaSequenceStaticData; }
		}

		public override ISequence CreateSequence()
		{
			return new TimedSequence();
		}

		public override ISequenceCache CreateSequenceCache()
		{
			throw new NotImplementedException();
		}

		public override IContentMigrator CreateMigrator()
		{
			return new SequenceMigrator();
		}

		public override ISequenceExecutor CreateExecutor()
		{
			return new Executor();
		}

		public override bool IsCustomSequenceLoader
		{
			get { return true; }
		}

		public override ISequence LoadSequenceFromFile(string lightOramaFile)
		{
			try
			{
				Logging.Info("LOR LoadSequenceFromFile");
				using (LightOramaSequenceImporterForm lightOramaFileImporterForm = new LightOramaSequenceImporterForm(lightOramaFile, StaticModuleData))
				{
					Logging.Info("LOR LoadSequenceFromFile show dialog");
					if (lightOramaFileImporterForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						return lightOramaFileImporterForm.Sequence;
					}
					else
					{
						//This will return a null sequence not sure we can do that.
						return null;
					}
				}
			}
			catch (Exception ex)
			{
				Logging.Info("LOR LoadSequenceFromFile caught exception " + ex.Message );
				return null;
			}
		}
	}
}