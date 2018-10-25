using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp.Auto
{
    public abstract class RLTrainingSettings
    {
        public abstract string DBInitializationSQL { get; private set; }
        public string DBFilename { get; set; } = "training.sqlite";
        public string ConnectionString => $"Data Source={DBFilename};Version=3;";
        public abstract string GetInsertRecordSQL(Move move, float score);
    }
}
