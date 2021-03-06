﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseWork.Models
{
    public class Balance
    {
        public int ID { get; set; }
        public string FK_PublicKey { get; set; } // связь должна быть по публичному ключу
        public string Asset { get; set; }
        public double Free { get; set; }
        public double Locked { get; set; }
        public APIKey APIKey { get; set; }
    }
}
