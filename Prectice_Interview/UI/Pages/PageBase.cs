using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public abstract class PageBase : UserControl
    {
        public abstract string PageTitle { get; }
        public virtual string PageSubtitle => "";
        public virtual Task RefreshAsync() => Task.CompletedTask;

        protected PageBase()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.Background;
            Font = Theme.Base;
            DoubleBuffered = true;
        }

        protected async Task Guard(Func<Task> action)
        {
            try { await action(); }
            catch (BusinessException ex) { UIx.Error(ex.Message); }
            catch (Exception ex)
            {
                Log.Error(ex, GetType().Name);
                UIx.Error("Something went wrong: " + (ex.InnerException?.Message ?? ex.Message) + "\r\n\r\nDetails were written to the log folder.");
            }
        }
    }
}
