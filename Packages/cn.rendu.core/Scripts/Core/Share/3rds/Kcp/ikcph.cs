// -----------------------------------------------------------------------------
// 第三方代码 —— 请勿按"自研代码"的方式随意重构
//
// 来源：KCP - A Fast and Reliable ARQ Protocol（https://github.com/skywind3000/kcp）
//   许可：MIT License（Copyright (c) 2020 by skywind3000）
//   本文件对应 KCP 的 C 头文件 ikcp.h，为 C# 移植版
//
// 本地改造：
//   1. 由 C 结构体重写为 C# StructLayout，命名空间归属 RD.Core
//   2. 结构体布局需与 ikcpc.cs 的 unsafe 指针操作严格对应，
//      调整字段顺序或大小会直接导致内存访问错误
//
// 升级提示：字段布局是对外契约，改动前须确认 ikcpc.cs 中所有指针运算
// 与偏移量均未失效。
// -----------------------------------------------------------------------------

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace RD.Core
{
    public delegate void KcpCallback(byte[] buffer, int length);

    internal unsafe struct IQUEUEHEAD
    {
        public IQUEUEHEAD* next;
        public IQUEUEHEAD* prev;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_init(IQUEUEHEAD* ptr)
        {
            ptr->next = ptr;
            ptr->prev = ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IKCPSEG* iqueue_entry(IQUEUEHEAD* ptr) => (IKCPSEG*)(byte*)(IKCPSEG*)ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool iqueue_is_empty(IQUEUEHEAD* entry) => entry == entry->next;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_del(IQUEUEHEAD* entry)
        {
            entry->next->prev = entry->prev;
            entry->prev->next = entry->next;
            entry->next = null;
            entry->prev = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_del_init(IQUEUEHEAD* entry)
        {
            iqueue_del(entry);
            iqueue_init(entry);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_add(IQUEUEHEAD* node, IQUEUEHEAD* head)
        {
            node->prev = head;
            node->next = head->next;
            head->next->prev = node;
            head->next = node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_add_tail(IQUEUEHEAD* node, IQUEUEHEAD* head)
        {
            node->prev = head->prev;
            node->next = head;
            head->prev->next = node;
            head->prev = node;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct IKCPSEG
    {
        public IQUEUEHEAD node;
        public uint conv;
        public uint cmd;
        public uint frg;
        public uint wnd;
        public uint ts;
        public uint sn;
        public uint una;
        public uint len;
        public uint resendts;
        public uint rto;
        public uint fastack;
        public uint xmit;
        public fixed byte data[1];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct IKCPCB
    {
        public uint conv, mtu, mss;
        public int state;
        public uint snd_una, snd_nxt, rcv_nxt;
        public uint ssthresh;
        public int rx_rttval, rx_srtt, rx_rto, rx_minrto;
        public uint snd_wnd, rcv_wnd, rmt_wnd, cwnd, probe;
        public uint current, interval, ts_flush, xmit;
        public uint nrcv_buf, nsnd_buf;
        public uint nrcv_que, nsnd_que;
        public uint nodelay, updated;
        public uint ts_probe, probe_wait;
        public uint incr;
        public IQUEUEHEAD snd_queue;
        public IQUEUEHEAD rcv_queue;
        public IQUEUEHEAD snd_buf;
        public IQUEUEHEAD rcv_buf;
        public uint* acklist;
        public uint ackcount;
        public uint ackblock;
        public int fastresend;
        public int fastlimit;
        public int nocwnd, stream;
    }

    public static class KCPBASIC
    {
        public const uint RTO_NDL = 30;
        public const uint RTO_MIN = 100;
        public const uint RTO_DEF = 200;
        public const uint RTO_MAX = 60000;
        public const uint CMD_PUSH = 81;
        public const uint CMD_ACK = 82;
        public const uint CMD_WASK = 83;
        public const uint CMD_WINS = 84;
        public const uint ASK_SEND = 1;
        public const uint ASK_TELL = 2;
        public const uint WND_SND = 32;
        public const uint WND_RCV = 128;
        public const uint MTU_DEF = 1400;
        public const uint ACK_FAST = 3;
        public const uint INTERVAL = 100;
        public const uint INTERVAL_MIN = 1;
        public const uint INTERVAL_LIMIT = 5000;
        public const uint OVERHEAD = 24;
        public const uint DEADLINK = 20;
        public const uint THRESH_INIT = 2;
        public const uint THRESH_MIN = 2;
        public const uint PROBE_INIT = 7000;
        public const uint PROBE_LIMIT = 120000;
        public const uint FRG_LIMIT = 255;
        public const uint NODELAY_MIN = 0;
        public const uint NODELAY_LIMIT = 2;
        public const uint FASTACK_MIN = 0;
        public const uint FASTACK_LIMIT = 5;
        public const uint OUTPUT = 1;
        public const uint INPUT = 2;
        public const uint SEND = 4;
        public const uint RECV = 8;
        public const uint IN_DATA = 16;
        public const uint IN_ACK = 32;
        public const uint IN_PROBE = 64;
        public const uint IN_WINS = 128;
        public const uint OUT_DATA = 256;
        public const uint OUT_ACK = 512;
        public const uint OUT_PROBE = 1024;
        public const uint OUT_WINS = 2048;

        // TODO: remove it if not needed
        public const uint REVERSED_HEAD = 5;
    }
}