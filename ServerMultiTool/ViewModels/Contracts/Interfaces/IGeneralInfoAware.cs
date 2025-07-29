using ServerMultiTool.ViewModels.Controls;

namespace ServerMultiTool.ViewModels.Contracts.Interfaces
{
    interface IGeneralInfoAware
    {
        public GeneralInfoViewModel GeneralInfo { get; set; }
    }
}
