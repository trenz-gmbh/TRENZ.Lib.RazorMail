using TRENZ.Lib.RazorMail.Models;

namespace TRENZ.Lib.RazorMail.SampleWebApi.Helpers;

public static class FileAttachmentHelper
{
    public static void GenerateAndAddDummyFileAttachmentsToMessage(MailMessage message, int fileSize, int fileAmount)
    {
        for (var i = 0; i < fileAmount; i++)
        {
            var attachment = CreateDummyFile(fileSize, i + 1);
            message.Content.Attachments.Add(attachment.FileName, attachment);
        }
    }

    private static MailAttachment CreateDummyFile(int fileSize, int nameIndex)
    {
        var fileSizeInMb = (int)Math.Floor(fileSize * 1e6);
        var dummyByteArray = new byte[fileSizeInMb];
        return new MailAttachment()
        {
            FileName = "DummyFile_" + nameIndex + ".txt",
            FileData = dummyByteArray,
            ContentType = "text/plain",
        };
    }
}
