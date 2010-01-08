/*
 * [The "BSD licence"]
 * Copyright (c) 2005 Martin Traverso
 * All rights reserved.
 *
 * Conversion to C#:
 * Copyright (c) 2008-2009 Sam Harwell, Pixel Mine, Inc.
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

namespace Antlr3.Targets
{
    using CodeGenerator = Antlr3.Codegen.CodeGenerator;
    using Target = Antlr3.Codegen.Target;

    public class RubyTarget : Target
    {
        public override string GetTargetCharLiteralFromANTLRCharLiteral(
                CodeGenerator generator,
                string literal )
        {
            literal = literal.Substring( 1, literal.Length - 2 );

            string result = "?";

            if ( literal.Equals( "\\" ) )
            {
                result += "\\\\";
            }
            else if ( literal.Equals( " " ) )
            {
                result += "\\s";
            }
            else if ( literal.StartsWith( "\\u" ) )
            {
                result = "0x" + literal.Substring( 2 );
            }
            else
            {
                result += literal;
            }

            return result;
        }

        public override int GetMaxCharValue( CodeGenerator generator )
        {
            // we don't support unicode, yet.
            return 0xFF;
        }

        public override string GetTokenTypeAsTargetLabel( CodeGenerator generator, int ttype )
        {
            string name = generator.grammar.GetTokenDisplayName( ttype );
            // If name is a literal, return the token type instead
            if ( name[0] == '\'' )
            {
                return generator.grammar.ComputeTokenNameFromLiteral( ttype, name );
            }
            return name;
        }
    }
}
