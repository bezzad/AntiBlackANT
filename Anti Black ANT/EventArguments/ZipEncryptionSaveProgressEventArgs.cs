using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventArguments
{
    public class ZipEncryptionSaveProgressEventArgs : EventArgs
    {
        private Ionic.Zip.SaveProgressEventArgs saveProgressEventArgs;
        private long totalFilesSize = 0;
        private double totalTransferredPercentForAllEntry = 0;
        private double totalTransferredPercentForCurrentEntry = 0;

        public Ionic.Zip.SaveProgressEventArgs SaveProgressEventArgs
        { get { return saveProgressEventArgs; } }
        public long TotalFilesSizeBytes
        { get { return totalFilesSize; } }
        public double TotalTransferredPercentForAllEntry
        {
            get { return totalTransferredPercentForAllEntry; }
        }
        public double TotalTransferredPercentForCurrentEntry
        {
            get { return totalTransferredPercentForCurrentEntry; }
        }

        public ZipEncryptionSaveProgressEventArgs(long FilesSize, 
            double TransferredPercentForAllEntry, 
            double TransferredPercentForCurrentEntry, 
            Ionic.Zip.SaveProgressEventArgs e)
        {
            this.saveProgressEventArgs = e;
            this.totalFilesSize = FilesSize;
            this.totalTransferredPercentForAllEntry = TransferredPercentForAllEntry;
            this.totalTransferredPercentForCurrentEntry = TransferredPercentForCurrentEntry;
        }
    }
}
