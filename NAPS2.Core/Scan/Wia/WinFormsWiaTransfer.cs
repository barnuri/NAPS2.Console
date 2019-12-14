using NAPS2.WinForms;
using System.IO;
using System.Windows.Forms;
using WIA;

namespace NAPS2.Scan.Wia
{
    public class WinFormsWiaTransfer : IWiaTransfer
    {
        private readonly IFormFactory formFactory;

        public WinFormsWiaTransfer(IFormFactory formFactory)
        {
            this.formFactory = formFactory;
        }

        public Stream Transfer(int pageNumber, WiaBackgroundEventLoop eventLoop, string format)
        {
            if (pageNumber == 1)
            {
                // The only downside of the common dialog is that it steals focus.
                // If this is the first page, then the user has just pressed the scan button, so that's not
                // an issue and we can use it and get the benefits of progress display and immediate cancellation.
                ImageFile imageFile = eventLoop.GetSync(wia =>
                    (ImageFile)new CommonDialogClass().ShowTransfer(wia.Item, format));
                if (imageFile == null)
                {
                    return null;
                }
                return new MemoryStream((byte[])imageFile.FileData.get_BinaryData());
            }
            // For subsequent pages, we don't want to take focus in case the user has switched applications,
            // so we use the custom form.
            var form = formFactory.Create<FScanProgress>();
            form.PageNumber = pageNumber;
            form.EventLoop = eventLoop;
            form.Format = format;
            form.ShowDialog();
            if (form.Exception != null)
            {
                throw form.Exception;
            }
            if (form.DialogResult == DialogResult.Cancel)
            {
                return null;
            }
            return form.ImageStream;
        }
    }
}