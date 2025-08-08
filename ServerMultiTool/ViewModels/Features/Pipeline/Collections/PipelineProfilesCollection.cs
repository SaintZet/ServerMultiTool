using ServerMultiTool.Model.Domain.Common;
using ServerMultiTool.Model.Domain.Pipeline;
using ServerMultiTool.Model.Features.ContinuousIntegration.GameServerLogs;
using ServerMultiTool.ViewModels.Wrappers.PipelineProfileWrappers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ServerMultiTool.ViewModels.Features.Pipeline.Collections
{
    public class PipelineProfilesCollection : ObservableCollection<PipelineProfileWrapper>
    {
        public PipelineProfilesCollection()
        : base()
        {

        }

        public PipelineProfilesCollection(IEnumerable<PipelineProfileWrapper> collection)
            : base(collection)
        {

        }

        public PipelineProfilesCollection Clone()
        {
            var clonedCollection = new ObservableCollection<PipelineProfileWrapper>();

            foreach (var profileWrapper in this)
            {
                var originalProfile = profileWrapper.ToOriginal();

                var clonedSteps = originalProfile.Steps.Select(step =>
                    new PipelineStep(step.Name, step.Description, step.Order)
                    {
                        Operations = [.. step.Operations]
                    }).ToList();

                var clonedProfile = new PipelineProfile(
                    originalProfile.Name,
                    originalProfile.Description,
                    clonedSteps);

                if (originalProfile.GsLogMonitoringSettings != null)
                {
                    var originalSettings = originalProfile.GsLogMonitoringSettings;
                    var clonedSettings = new GsLogMonitoringSettings
                    {
                        Enable = originalSettings.Enable,
                        MasterLogDirectory = originalSettings.MasterLogDirectory != null ?
                            new DirectoryModel(originalSettings.MasterLogDirectory.Path) : null,
                        SegmentLogDirectory = originalSettings.SegmentLogDirectory != null ?
                            new DirectoryModel(originalSettings.SegmentLogDirectory.Path) : null
                    };
                    clonedProfile.UpdateGsLogMonitoringSettings(clonedSettings);
                }

                var clonedWrapper = new PipelineProfileWrapper(clonedProfile);
                clonedCollection.Add(clonedWrapper);
            }

            return new PipelineProfilesCollection(clonedCollection);
        }
    }
}
