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

namespace StringTemplate.Debug
{
    using System.Collections.Generic;
    using CultureInfo = System.Globalization.CultureInfo;
    using StringWriter = System.IO.StringWriter;

    public class DebugTemplate : Template
    {
        public DebugTemplate()
        {
            this.InterpreterEvents = new List<InterpEvent>();
            this.AddAttributeEvents = new Dictionary<string, ICollection<AddAttributeEvent>>();
            this.TemplateConstructionEvent = new ConstructionEvent();
        }

        public IList<InterpEvent> InterpreterEvents
        {
            get;
            private set;
        }

        public IDictionary<string, ICollection<AddAttributeEvent>> AddAttributeEvents
        {
            get;
            private set;
        }

        public ConstructionEvent TemplateConstructionEvent
        {
            get;
            private set;
        }

        public override void Add(string name, object value)
        {
            if (groupThatCreatedThisInstance.Debug)
            {
                ICollection<AddAttributeEvent> collection;
                if (!AddAttributeEvents.TryGetValue(name, out collection))
                {
                    collection = new List<AddAttributeEvent>();
                    AddAttributeEvents[name] = collection;
                }

                collection.Add(new AddAttributeEvent(name, value));
            }

            base.Add(name, value);
        }

        public IList<InterpEvent> GetEvents()
        {
            return GetEvents(CultureInfo.CurrentCulture, AutoIndentWriter.NoWrap);
        }

        public IList<InterpEvent> GetEvents(int lineWidth)
        {
            return GetEvents(CultureInfo.CurrentCulture, lineWidth);
        }

        public IList<InterpEvent> GetEvents(CultureInfo culture)
        {
            return GetEvents(CultureInfo.CurrentCulture, AutoIndentWriter.NoWrap);
        }

        public IList<InterpEvent> GetEvents(CultureInfo culture, int lineWidth)
        {
            StringWriter stream = new StringWriter();
            ITemplateWriter writer = new AutoIndentWriter(stream);
            writer.SetLineWidth(lineWidth);
            Interpreter interp = new Interpreter(groupThatCreatedThisInstance, culture);
            interp.Exec(writer, this);
            return interp.Events;
        }
    }
}
