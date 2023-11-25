using QuizApp.Services.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.TriggerAction
{
    public class Program
    {
        static void Main(String[] args)
        {
            TriggerExternalItem();
        }

        static void TriggerExternalItem()
        {
            //GetBufferedQueueItems()
            ExternalActionQueueService.TriggerAction();
        }
    }
}
