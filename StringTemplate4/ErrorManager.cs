﻿/*
 * [The "BSD licence"]
 * Copyright (c) 2005-2008 Terence Parr
 * All rights reserved.
 *
 * Conversion to C#:
 * Copyright (c) 2008-2009 Sam Harwell
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace StringTemplate
{
    using Console = System.Console;
    using Exception = System.Exception;
    using IOException = System.IO.IOException;
    using ThreadStatic = System.ThreadStaticAttribute;
    using StringTemplate.Compiler;
    using Type = System.Type;
    using Antlr.Runtime;

    public static class ErrorManager
    {
        public static readonly ITemplateErrorListener DefaultErrorListener = new DefaultErrorListenerImpl();

        [ThreadStatic]
        private static ITemplateErrorListener listener;

        /** Backward compatibility for tombu, co-designer.  Don't check missing
         *  args against formal arg lists and don't require template headers in .st
         *  files.
         */
        public static bool CompatibilityMode
        {
            get;
            set;
        }

        public static ITemplateErrorListener ErrorListener
        {
            get
            {
                return listener ?? DefaultErrorListener;
            }
            set
            {
                listener = value;
            }
        }

        private static Type[] CriticalExceptions =
            {
                typeof(System.StackOverflowException),
                typeof(System.OutOfMemoryException),
                typeof(System.Threading.ThreadAbortException),
                typeof(System.Runtime.InteropServices.SEHException),
                typeof(System.Security.SecurityException),
                typeof(System.ExecutionEngineException),
                typeof(System.AccessViolationException),
                typeof(System.BadImageFormatException),
                typeof(System.AppDomainUnloadedException),
            };

        public static bool IsCriticalException(Exception ex)
        {
            if (ex == null)
                return false;

            var exceptionType = ex.GetType();
            foreach (Type t in CriticalExceptions)
            {
                if (t.IsAssignableFrom(exceptionType))
                    return true;
            }

            return false;
        }

        public static void CompileTimeError(ErrorType error, object arg)
        {
            ErrorListener.CompileTimeError(new TemplateMessage(error, null, null, arg));
        }

        public static void CompileTimeError(ErrorType error, object arg1, object arg2)
        {
            ErrorListener.CompileTimeError(new TemplateMessage(error, null, null, arg1, arg2));
        }

        public static void SyntaxError(ErrorType error, RecognitionException e, string msg)
        {
            ErrorListener.CompileTimeError(new TemplateCompileTimeMessage(error, e.Token, e, msg));
        }

        public static void SyntaxError(ErrorType error, RecognitionException e, string msg, object arg)
        {
            ErrorListener.CompileTimeError(new TemplateCompileTimeMessage(error, e.Token, e, msg, arg));
        }

        public static void RuntimeError(Template template, int ip, ErrorType error)
        {
            ErrorListener.RuntimeError(new TemplateRuntimeMessage(error, ip, template));
        }

        public static void RuntimeError(Template template, int ip, ErrorType error, object arg)
        {
            ErrorListener.RuntimeError(new TemplateRuntimeMessage(error, ip, template, null, arg));
        }

        public static void RuntimeError(Template template, int ip, ErrorType error, Exception source, object arg)
        {
            ErrorListener.RuntimeError(new TemplateRuntimeMessage(error, ip, template, source, arg));
        }

        public static void RuntimeError(Template template, int ip, ErrorType error, object arg1, object arg2)
        {
            ErrorListener.RuntimeError(new TemplateRuntimeMessage(error, ip, template, null, arg1, arg2));
        }

        public static void IOError(Template template, ErrorType error, Exception source)
        {
            ErrorListener.IOError(new TemplateMessage(error, template, source));
        }

        public static void IOError(Template template, ErrorType error, Exception source, object arg)
        {
            ErrorListener.IOError(new TemplateMessage(error, template, source, arg));
        }

        public static void InternalError(Template template, ErrorType error, Exception source)
        {
            ErrorListener.InternalError(new TemplateMessage(error, template, source));
        }

        public static void InternalError(Template template, ErrorType error, Exception source, object arg)
        {
            ErrorListener.InternalError(new TemplateMessage(error, template, source, arg));
        }

        public static void InternalError(Template template, ErrorType error, Exception source, object arg1, object arg2)
        {
            ErrorListener.InternalError(new TemplateMessage(error, template, source, arg1, arg2));
        }

        private class DefaultErrorListenerImpl : ITemplateErrorListener
        {
            public void CompileTimeError(TemplateMessage message)
            {
            }

            public void RuntimeError(TemplateMessage message)
            {
            }

            public void IOError(TemplateMessage message)
            {
                throw new IOException(message.Message, message.Source);
            }

            public void InternalError(TemplateMessage message)
            {
                throw new TemplateException(message.Message, message.Source);
            }

            // TODO: put in [root ... template] stack
            public void Error(string message, Exception e)
            {
                Console.Error.WriteLine(message);
                if (e != null)
                    Console.Error.WriteLine(e.StackTrace);
            }

            public void Error(string message)
            {
                Error(message, null);
            }

            public void Warning(string message)
            {
                Console.WriteLine(message);
            }
        }
    }
}
