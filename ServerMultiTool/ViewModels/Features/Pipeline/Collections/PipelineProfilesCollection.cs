using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ServerMultiTool.Model.Features.Pipeline.Profile;
using ServerMultiTool.Model.Features.Pipeline.Step;
using ServerMultiTool.ViewModels.Features.Pipeline.Wrappers.PipelineProfile;

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
                    originalProfile.Id,
                    originalProfile.Name,
                    originalProfile.Description,
                    clonedSteps);


                var clonedWrapper = new PipelineProfileWrapper(clonedProfile);
                clonedCollection.Add(clonedWrapper);
            }

            return new PipelineProfilesCollection(clonedCollection);
        }
    }
}
