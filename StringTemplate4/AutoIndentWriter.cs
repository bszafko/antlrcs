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
    using System.Collections.Generic;
    using Array = System.Array;
    using Environment = System.Environment;
    using StringBuilder = System.Text.StringBuilder;
    using TextWriter = System.IO.TextWriter;
    using InvalidOperationException = System.InvalidOperationException;
    using ArgumentNullException = System.ArgumentNullException;

    public class AutoIndentWriter : ITemplateWriter
    {
        public static readonly int NoWrap = -1;

        /** stack of indents; use List as it's much faster than Stack. Grows
         *  from 0..n-1.
         */
        protected readonly IList<string> indents = new List<string>();

        /** Stack of integer anchors (char positions in line); avoid Integer
         *  creation overhead.
         */
        protected int[] anchors = new int[10];
        protected int anchors_sp = -1;

        /** \n or \r\n? */
        protected readonly string newline;

        protected readonly TextWriter @out = null;
        protected bool atStartOfLine = true;

        /** Track char position in the line (later we can think about tabs).
         *  Indexed from 0.  We want to keep charPosition &lt;= lineWidth.
         *  This is the position we are *about* to write not the position
         *  last written to.
         */
        protected int charPosition = 0;

        /// <summary>
        /// The absolute char index into the output of the next char to be written.
        /// </summary>
        protected int charIndex = 0;

        protected int lineWidth = NoWrap;

        protected int charPositionOfStartOfExpr = 0;

        public AutoIndentWriter(TextWriter @out, string newline)
        {
            if (@out == null)
                throw new ArgumentNullException("out");

            this.@out = @out;
            indents.Add(null); // s oftart with no indent
            this.newline = newline ?? Environment.NewLine;
        }

        public AutoIndentWriter(TextWriter @out)
            : this(@out, Environment.NewLine)
        {
        }

        public int Index
        {
            get
            {
                return charIndex;
            }
        }

        public virtual void SetLineWidth(int lineWidth)
        {
            this.lineWidth = lineWidth;
        }

        public virtual void PushIndentation(string indent)
        {
            indents.Add(indent);
        }

        public virtual string PopIndentation()
        {
            if (indents.Count == 0)
                throw new InvalidOperationException();

            var result = indents[indents.Count - 1];
            indents.RemoveAt(indents.Count - 1);
            return result;
        }

        public virtual void PushAnchorPoint()
        {
            if ((anchors_sp + 1) >= anchors.Length)
            {
                Array.Resize(ref anchors, anchors.Length * 2);
            }
            anchors_sp++;
            anchors[anchors_sp] = charPosition;
        }

        public virtual void PopAnchorPoint()
        {
            if (anchors_sp == -1)
                throw new InvalidOperationException();

            anchors_sp--;
        }

        public virtual int GetIndentationWidth()
        {
            int n = 0;
            for (int i = 0; i < indents.Count; i++)
            {
                string ind = indents[i];
                if (ind != null)
                {
                    n += ind.Length;
                }
            }
            return n;
        }

        /** Write out a string literal or attribute expression or expression element.*/
        public virtual int Write(string str)
        {
            if (str == null)
                return 0;

            int n = 0;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                // found \n or \r\n newline?
                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    atStartOfLine = true;
                    charPosition = -1; // set so the write below sets to 0
                    @out.Write(newline);
                    n += newline.Length;
                    charIndex += newline.Length;
                    charPosition += n; // wrote n more char
                    continue;
                }
                // normal character
                // check to see if we are at the start of a line; need indent if so
                if (atStartOfLine)
                {
                    n += Indent();
                    atStartOfLine = false;
                }
                n++;
                @out.Write(c);
                charPosition++;
                charIndex++;
            }
            return n;
        }

        public virtual int WriteSeparator(string str)
        {
            return Write(str);
        }

        /** Write out a string literal or attribute expression or expression element.
         *
         *  If doing line wrap, then check wrap before emitting this str.  If
         *  at or beyond desired line width then emit a \n and any indentation
         *  before spitting out this str.
         */
        public virtual int Write(string str, string wrap)
        {
            int n = WriteWrap(wrap);
            return n + Write(str);
        }

        public virtual int WriteWrap(string wrap)
        {
            int n = 0;
            // if want wrap and not already at start of line (last char was \n)
            // and we have hit or exceeded the threshold
            if (lineWidth != NoWrap && wrap != null && !atStartOfLine &&
                 charPosition >= lineWidth)
            {
                // ok to wrap
                // Walk wrap string and look for A\nB.  Spit out A\n
                // then spit indent or anchor, whichever is larger
                // then spit out B.
                for (int i = 0; i < wrap.Length; i++)
                {
                    char c = wrap[i];
                    if (c == '\n')
                    {
                        @out.Write(newline);
                        n += newline.Length;
                        charPosition = 0;
                        charIndex += newline.Length;
                        n += Indent();
                        // continue writing any chars out
                    }
                    else
                    {  // write A or B part
                        n++;
                        @out.Write(c);
                        charPosition++;
                        charIndex++;
                    }
                }
            }
            return n;
        }

        public virtual int Indent()
        {
            int n = 0;
            for (int i = 0; i < indents.Count; i++)
            {
                string ind = indents[i];
                if (ind != null)
                {
                    n += ind.Length;
                    @out.Write(ind);
                }
            }

            // If current anchor is beyond current indent width, indent to anchor
            // *after* doing indents (might tabs in there or whatever)
            int indentWidth = n;
            if (anchors_sp >= 0 && anchors[anchors_sp] > indentWidth)
            {
                int remainder = anchors[anchors_sp] - indentWidth;
                for (int i = 1; i <= remainder; i++)
                    @out.Write(' ');
                n += remainder;
            }

            charPosition += n;
            charIndex += n;
            return n;
        }

        protected virtual StringBuilder GetIndentString(int spaces)
        {
            StringBuilder buf = new StringBuilder();
            for (int i = 1; i <= spaces; i++)
            {
                buf.Append(' ');
            }
            return buf;
        }
    }
}
