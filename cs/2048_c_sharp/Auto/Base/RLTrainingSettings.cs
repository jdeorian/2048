using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048_c_sharp.Auto
{
    public class RLTrainingSettings
    {
        public string DBInitializationSQL { get; set; }
        public string DBFilename { get; set; } = "training.sqlite";
        public string ConnectionString => $"Data Source={DBFilename};Version=3;";
    }
}
