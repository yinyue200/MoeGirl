using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using yfxsApp.Common;

namespace yfxsApp.runtime
{
    /// <summary>
    /// 表示可以作为内部页的页
    /// </summary>
    interface IRestPage
    {
        /// <summary>
        /// 保存内部页的数据
        /// </summary>
        void SaveData();
    }
    /// <summary>
    /// 使用 <see cref="yfxsApp.Common.NavigationHelper"/> 实现基本导航功能的内部页
    /// </summary>
    public class RestPage:BasicPage,IRestPage
    {
        public RestPage ():base(null)
        {
            ReSet(new NavigationHelper(this, false));
        }
        public void SaveData()
        {
            NavigationHelper.OnNavigatedFrom();
        }
    }
    /// <summary>
    /// 使用 <see cref="yfxsApp.Common.NavigationHelper"/> 实现基本导航功能的空白页
    /// </summary>
    public class BasicPage:Page
    {
        private NavigationHelper navigationHelper;
        /// <summary>
        /// 获取与此 <see cref="Page"/> 关联的 <see cref="NavigationHelper"/>。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        public BasicPage ()
        {
            ReSet();
        }
        protected void ReSet()
        {
            ReSet(new NavigationHelper(this));
        }
        public BasicPage (NavigationHelper helper)
        {
            if(helper !=null)
            {
                ReSet(helper);
            }
        }
        protected void ReSet(NavigationHelper helper)
        {
            this.navigationHelper = helper;
            this.navigationHelper.LoadState += NavigationHelper_LoadState;
            this.navigationHelper.SaveState += NavigationHelper_SaveState;
        }
        /// <summary>
        /// 保留与此页关联的状态，以防挂起应用程序或
        /// 从导航缓存中放弃此页。值必须符合
        /// <see cref="SuspensionManager.SessionState"/> 的序列化要求。
        /// </summary>
        /// <param name="sender">事件的来源；通常为 <see cref="NavigationHelper"/></param>
        ///<param name="e">提供要使用可序列化状态填充的空字典
        ///的事件数据。</param>
        protected virtual void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }
        /// <summary>
        /// 使用在导航过程中传递的内容填充页。  在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源; 通常为 <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 字典。 首次访问页面时，该状态将为 null。</param>
        protected virtual void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
    }
}
