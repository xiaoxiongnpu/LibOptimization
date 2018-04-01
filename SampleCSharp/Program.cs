﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibOptimization.Optimization;
using LibOptimization.Util;

namespace SampleCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Typical use
            {
                //Target Function
                var func = new RosenBrock(2);

                //Set Function
                var opt = new LibOptimization.Optimization.clsOptNelderMead(func);
                opt.Init();

                //Optimization
                opt.DoIteration();

                //Check Error
                if(opt.IsRecentError()==true)
                {
                    return;
                }
                else
                {
                    //Get Result
                    clsUtil.DebugValue(opt);
                }
            }

            //Evaluate optimization result per 100 iteration
            {
                var opt = new LibOptimization.Optimization.clsOptDEJADE(new RosenBrock(10));
                opt.Init();
                clsUtil.DebugValue(opt);

                while (opt.DoIteration(100)==false)
                {
                    clsUtil.DebugValue(opt, ai_isOutValue: false);
                }
                clsUtil.DebugValue(opt);
            }

            //Evaluate optimization result per 100 iteration with check my criterion.
            {
                var opt = new LibOptimization.Optimization.clsOptDEJADE(new RosenBrock(10));
                //Disable Internal criterion
                opt.IsUseCriterion = false;
                //Init
                opt.Init();
                clsUtil.DebugValue(opt);

                while (opt.DoIteration(100) == false)
                {
                    var eval = opt.Result.Eval;

                    //my criterion
                    if (eval < 0.01)
                    {
                        break;
                    }
                    else
                    {
                        clsUtil.DebugValue(opt, ai_isOutValue: false);
                    }
                }
                clsUtil.DebugValue(opt);
            }
        }
    }
}
