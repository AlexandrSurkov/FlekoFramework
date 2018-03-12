namespace Flekosoft.Common.Plugins.Windows
{
    public interface IWinFormsControlPlugin : IPlugin
    {
        System.Windows.Forms.ContainerControl GetControl(object instance);
    }
}
