using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GemDriver
{
    class Release
    {
        //2025/6/09 修改内容：1.关闭线程，关闭所有data刷新及SECS GEM界面内容显示
        //2025/6/19 修改内容：1.上报Hose Al警报,使能警报0x80，清除警报0x00；2.修改于RGA软件的AL触发模式，由GRA软件发送“0，0，0，1，0”字串触发，改为ALID扩展了一下，11个超阈值每一个都对应一个AL MSG格式: CH1 # Exceeding the # threshold(#) - is #我填充的例子：CH1 N2/H2P Exceeding the lower threshold(0.6) - is 0.5
                    //实际通讯中我发送的例如 "RND,ALID,CH1 N2/H2P Exceeding the lower threshold(0.6) - is 0.5,CH2 N2/H2P Exceeding the lower threshold(0.6) - is 0.5"仍然使用','分隔；3.添加读取机台slid door状态，发送给RGA，RGA软件触发警报时添加到MSG讯息中； 
    }
}
