namespace Show
{
    public class DebugViewModel : BaseViewModel
    {
        private string text = "";

        public string Text
        {
            get { return text; }
            set { text = value; OnPropertyChanged(nameof(Text)); }
        }
    }
}
