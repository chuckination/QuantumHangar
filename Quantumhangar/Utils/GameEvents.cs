﻿using Sandbox;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace QuantumHangar.Utilities
{
    public static class GameEvents
    {
        /*  This is just used to house some basic invokeongamethread calls. (Based off of Jimms code)
         */
        public static Task<bool> InvokeActionAsync(Action action, [CallerMemberName] string caller = "")
        {
            //Jimm thank you. This is the best


            var ctx = new TaskCompletionSource<bool>();
            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    action.Invoke();
                    ctx.SetResult(true);
                }
                catch (Exception e)
                {
                    ctx.SetException(e);
                }

            }, caller);
            return ctx.Task;
        }


        public static Task<T> InvokeAsync<T>(Func<T> action, [CallerMemberName] string caller = "")
        {
            //Jimm thank you. This is the best
            var ctx = new TaskCompletionSource<T>();
            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    ctx.SetResult(action.Invoke());
                }
                catch (Exception e)
                {
                    ctx.SetException(e);
                }

            }, caller);
            return ctx.Task;
        }

        public static Task<T2> InvokeAsync<T1, T2>(Func<T1, T2> action, T1 arg, [CallerMemberName] string caller = "")
        {
            //Jimm thank you. This is the best
            var ctx = new TaskCompletionSource<T2>();
            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    ctx.SetResult(action.Invoke(arg));
                }
                catch (Exception e)
                {
                    ctx.SetException(e);
                }

            }, caller);
            return ctx.Task;
        }

        public static Task<T3> InvokeAsync<T1, T2, T3>(Func<T1, T2, T3> action, T1 arg, T2 arg2, [CallerMemberName] string caller = "")
        {
            //Jimm thank you. This is the best
            var ctx = new TaskCompletionSource<T3>();

            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    ctx.SetResult(action.Invoke(arg, arg2));
                }
                catch (Exception e)
                {
                    ctx.SetException(e);
                }

            }, caller);
            return ctx.Task;
        }

        public static Task<T4> InvokeAsync<T1, T2, T3, T4>(Func<T1, T2, T3, T4> action, T1 arg, T2 arg2, T3 arg3, [CallerMemberName] string caller = "")
        {
            //Jimm thank you. This is the best
            var ctx = new TaskCompletionSource<T4>();
            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    ctx.SetResult(action.Invoke(arg, arg2, arg3));
                }
                catch (Exception e)
                {
                    ctx.SetException(e);
                }

            }, caller);
            return ctx.Task;
        }


        public static Task<T6> InvokeAsync<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6> action, T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5, [CallerMemberName] string caller = "")
        {
            //Jimm thank you. This is the best
            var ctx = new TaskCompletionSource<T6>();
            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    ctx.SetResult(action.Invoke(arg, arg2, arg3, arg4, arg5));
                }
                catch (Exception e)
                {
                    ctx.SetException(e);
                }

            }, caller);
            return ctx.Task;
        }
    }
}
