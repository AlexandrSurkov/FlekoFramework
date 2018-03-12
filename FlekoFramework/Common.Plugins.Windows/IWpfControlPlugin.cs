namespace Flekosoft.Common.Plugins.Windows
{
    public interface IWpfControlPlugin : IPlugin
    {
        System.Windows.Controls.ContentControl GetControl(object instance);
    }
}
