using System;
using System.Collections.Generic;
using System.Linq;
using Vixen.Module;
using System.Runtime.Serialization;

namespace VixenModules.SequenceType.LightOrama
{
	[DataContract]
	public class LightOramaSequenceStaticData : ModuleDataModelBase
	{
		[DataMember] private Dictionary<string, List<LorChannelMapping>> m_LightOramaMappings;

		public Dictionary<string, List<LorChannelMapping>> LightOramaMappings
		{
			get
			{
				if (m_LightOramaMappings == null)
					m_LightOramaMappings = new Dictionary<string, List<LorChannelMapping>>();
				return m_LightOramaMappings;
			}
			set { m_LightOramaMappings = value; }
		}

		public LightOramaSequenceStaticData()
		{
			m_LightOramaMappings = new Dictionary<string, List<LorChannelMapping>>();
		}

		public override IModuleDataModel Clone()
		{
			LightOramaSequenceStaticData data = new LightOramaSequenceStaticData();
			data.m_LightOramaMappings = new Dictionary<string, List<LorChannelMapping>>(m_LightOramaMappings);
			return data;
		}
	}
}