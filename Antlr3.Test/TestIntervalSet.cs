/*
 * [The "BSD licence"]
 * Copyright (c) 2005-2008 Terence Parr
 * All rights reserved.
 *
 * Conversion to C#:
 * Copyright (c) 2008 Sam Harwell, Pixel Mine, Inc.
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

namespace AntlrUnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using IList = System.Collections.IList;
    using IntervalSet = Antlr3.Misc.IntervalSet;
    using Label = Antlr3.Analysis.Label;

    [TestClass]
    public class TestIntervalSet : BaseTest
    {

        /** Public default constructor used by TestRig */
        public TestIntervalSet()
        {
        }

        [TestMethod]
        public void TestSingleElement() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 99 );
            string expecting = "99";
            assertEquals( s.ToString(), expecting );
        }

        [TestMethod]
        public void TestIsolatedElements() /*throws Exception*/ {
            IntervalSet s = new IntervalSet();
            s.Add( 1 );
            s.Add( 'z' );
            s.Add( '\uFFF0' );
            string expecting = "{1, 122, 65520}";
            assertEquals( s.ToString(), expecting );
        }

        [TestMethod]
        public void TestMixedRangesAndElements() /*throws Exception*/ {
            IntervalSet s = new IntervalSet();
            s.Add( 1 );
            s.Add( 'a', 'z' );
            s.Add( '0', '9' );
            string expecting = "{1, 48..57, 97..122}";
            assertEquals( s.ToString(), expecting );
        }

        [TestMethod]
        public void TestSimpleAnd() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 10, 20 );
            IntervalSet s2 = IntervalSet.Of( 13, 15 );
            string expecting = "13..15";
            string result = ( s.And( s2 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestRangeAndIsolatedElement() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 'a', 'z' );
            IntervalSet s2 = IntervalSet.Of( 'd' );
            string expecting = "100";
            string result = ( s.And( s2 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestEmptyIntersection() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 'a', 'z' );
            IntervalSet s2 = IntervalSet.Of( '0', '9' );
            string expecting = "{}";
            string result = ( s.And( s2 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestEmptyIntersectionSingleElements() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 'a' );
            IntervalSet s2 = IntervalSet.Of( 'd' );
            string expecting = "{}";
            string result = ( s.And( s2 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestNotSingleElement() /*throws Exception*/ {
            IntervalSet vocabulary = IntervalSet.Of( 1, 1000 );
            vocabulary.Add( 2000, 3000 );
            IntervalSet s = IntervalSet.Of( 50, 50 );
            string expecting = "{1..49, 51..1000, 2000..3000}";
            string result = ( s.Complement( vocabulary ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestNotSet() /*throws Exception*/ {
            IntervalSet vocabulary = IntervalSet.Of( 1, 1000 );
            IntervalSet s = IntervalSet.Of( 50, 60 );
            s.Add( 5 );
            s.Add( 250, 300 );
            string expecting = "{1..4, 6..49, 61..249, 301..1000}";
            string result = ( s.Complement( vocabulary ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestNotEqualSet() /*throws Exception*/ {
            IntervalSet vocabulary = IntervalSet.Of( 1, 1000 );
            IntervalSet s = IntervalSet.Of( 1, 1000 );
            string expecting = "{}";
            string result = ( s.Complement( vocabulary ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestNotSetEdgeElement() /*throws Exception*/ {
            IntervalSet vocabulary = IntervalSet.Of( 1, 2 );
            IntervalSet s = IntervalSet.Of( 1 );
            string expecting = "2";
            string result = ( s.Complement( vocabulary ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestNotSetFragmentedVocabulary() /*throws Exception*/ {
            IntervalSet vocabulary = IntervalSet.Of( 1, 255 );
            vocabulary.Add( 1000, 2000 );
            vocabulary.Add( 9999 );
            IntervalSet s = IntervalSet.Of( 50, 60 );
            s.Add( 3 );
            s.Add( 250, 300 );
            s.Add( 10000 ); // this is outside range of vocab and should be ignored
            string expecting = "{1..2, 4..49, 61..249, 1000..2000, 9999}";
            string result = ( s.Complement( vocabulary ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestSubtractOfCompletelyContainedRange() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 10, 20 );
            IntervalSet s2 = IntervalSet.Of( 12, 15 );
            string expecting = "{10..11, 16..20}";
            string result = ( s.Subtract( s2 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestSubtractOfOverlappingRangeFromLeft() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 10, 20 );
            IntervalSet s2 = IntervalSet.Of( 5, 11 );
            string expecting = "12..20";
            string result = ( s.Subtract( s2 ) ).ToString();
            assertEquals( result, expecting );

            IntervalSet s3 = IntervalSet.Of( 5, 10 );
            expecting = "11..20";
            result = ( s.Subtract( s3 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestSubtractOfOverlappingRangeFromRight() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 10, 20 );
            IntervalSet s2 = IntervalSet.Of( 15, 25 );
            string expecting = "10..14";
            string result = ( s.Subtract( s2 ) ).ToString();
            assertEquals( result, expecting );

            IntervalSet s3 = IntervalSet.Of( 20, 25 );
            expecting = "10..19";
            result = ( s.Subtract( s3 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestSubtractOfCompletelyCoveredRange() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 10, 20 );
            IntervalSet s2 = IntervalSet.Of( 1, 25 );
            string expecting = "{}";
            string result = ( s.Subtract( s2 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestSubtractOfRangeSpanningMultipleRanges() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 10, 20 );
            s.Add( 30, 40 );
            s.Add( 50, 60 ); // s has 3 ranges now: 10..20, 30..40, 50..60
            IntervalSet s2 = IntervalSet.Of( 5, 55 ); // covers one and touches 2nd range
            string expecting = "56..60";
            string result = ( s.Subtract( s2 ) ).ToString();
            assertEquals( result, expecting );

            IntervalSet s3 = IntervalSet.Of( 15, 55 ); // touches both
            expecting = "{10..14, 56..60}";
            result = ( s.Subtract( s3 ) ).ToString();
            assertEquals( result, expecting );
        }

        /** The following was broken:
            {0..113, 115..65534}-{0..115, 117..65534}=116..65534
         */
        [TestMethod]
        public void TestSubtractOfWackyRange() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 0, 113 );
            s.Add( 115, 200 );
            IntervalSet s2 = IntervalSet.Of( 0, 115 );
            s2.Add( 117, 200 );
            string expecting = "116";
            string result = ( s.Subtract( s2 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestSimpleEquals() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 10, 20 );
            IntervalSet s2 = IntervalSet.Of( 10, 20 );
            Boolean expecting = true;
            Boolean result = s.Equals( s2 );
            assertEquals( result, expecting );

            IntervalSet s3 = IntervalSet.Of( 15, 55 );
            expecting = false;
            result = s.Equals( s3 );
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestEquals() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 10, 20 );
            s.Add( 2 );
            s.Add( 499, 501 );
            IntervalSet s2 = IntervalSet.Of( 10, 20 );
            s2.Add( 2 );
            s2.Add( 499, 501 );
            Boolean expecting = true;
            Boolean result = s.Equals( s2 );
            assertEquals( result, expecting );

            IntervalSet s3 = IntervalSet.Of( 10, 20 );
            s3.Add( 2 );
            expecting = false;
            result = s.Equals( s3 );
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestSingleElementMinusDisjointSet() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 15, 15 );
            IntervalSet s2 = IntervalSet.Of( 1, 5 );
            s2.Add( 10, 20 );
            string expecting = "{}"; // 15 - {1..5, 10..20} = {}
            string result = s.Subtract( s2 ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestMembership() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 15, 15 );
            s.Add( 50, 60 );
            assertTrue( !s.Contains( 0 ) );
            assertTrue( !s.Contains( 20 ) );
            assertTrue( !s.Contains( 100 ) );
            assertTrue( s.Contains( 15 ) );
            assertTrue( s.Contains( 55 ) );
            assertTrue( s.Contains( 50 ) );
            assertTrue( s.Contains( 60 ) );
        }

        // {2,15,18} & 10..20
        [TestMethod]
        public void TestIntersectionWithTwoContainedElements() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 10, 20 );
            IntervalSet s2 = IntervalSet.Of( 2, 2 );
            s2.Add( 15 );
            s2.Add( 18 );
            string expecting = "{15, 18}";
            string result = ( s.And( s2 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestIntersectionWithTwoContainedElementsReversed() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 10, 20 );
            IntervalSet s2 = IntervalSet.Of( 2, 2 );
            s2.Add( 15 );
            s2.Add( 18 );
            string expecting = "{15, 18}";
            string result = ( s2.And( s ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestComplement() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 100, 100 );
            s.Add( 101, 101 );
            IntervalSet s2 = IntervalSet.Of( 100, 102 );
            string expecting = "102";
            string result = ( s.Complement( s2 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestComplement2() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 100, 101 );
            IntervalSet s2 = IntervalSet.Of( 100, 102 );
            string expecting = "102";
            string result = ( s.Complement( s2 ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestComplement3() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 1, 96 );
            s.Add( 99, Label.MAX_CHAR_VALUE );
            string expecting = "97..98";
            string result = ( s.Complement( 1, Label.MAX_CHAR_VALUE ) ).ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestMergeOfRangesAndSingleValues() /*throws Exception*/ {
            // {0..41, 42, 43..65534}
            IntervalSet s = IntervalSet.Of( 0, 41 );
            s.Add( 42 );
            s.Add( 43, 65534 );
            string expecting = "0..65534";
            string result = s.ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestMergeOfRangesAndSingleValuesReverse() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 43, 65534 );
            s.Add( 42 );
            s.Add( 0, 41 );
            string expecting = "0..65534";
            string result = s.ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestMergeWhereAdditionMergesTwoExistingIntervals() /*throws Exception*/ {
            // 42, 10, {0..9, 11..41, 43..65534}
            IntervalSet s = IntervalSet.Of( 42 );
            s.Add( 10 );
            s.Add( 0, 9 );
            s.Add( 43, 65534 );
            s.Add( 11, 41 );
            string expecting = "0..65534";
            string result = s.ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestMergeWithDoubleOverlap() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 1, 10 );
            s.Add( 20, 30 );
            s.Add( 5, 25 ); // overlaps two!
            string expecting = "1..30";
            string result = s.ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestSize() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 20, 30 );
            s.Add( 50, 55 );
            s.Add( 5, 19 );
            string expecting = "32";
            string result = s.Count.ToString();
            assertEquals( result, expecting );
        }

        [TestMethod]
        public void TestToList() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 20, 25 );
            s.Add( 50, 55 );
            s.Add( 5, 5 );
            string expecting = "[5, 20, 21, 22, 23, 24, 25, 50, 51, 52, 53, 54, 55]";
            IList foo = new List<object>();
            //String result = String.valueOf( s.toList() );
            string result = "[" + string.Join( ", ", s.ToArray().Select( i => i.ToString() ).ToArray() ) + "]";
            assertEquals( result, expecting );
        }

        /** The following was broken:
            {'\u0000'..'s', 'u'..'\uFFFE'} & {'\u0000'..'q', 's'..'\uFFFE'}=
            {'\u0000'..'q', 's'}!!!! broken...
            'q' is 113 ascii
            'u' is 117
        */
        [TestMethod]
        public void TestNotRIntersectionNotT() /*throws Exception*/ {
            IntervalSet s = IntervalSet.Of( 0, 's' );
            s.Add( 'u', 200 );
            IntervalSet s2 = IntervalSet.Of( 0, 'q' );
            s2.Add( 's', 200 );
            string expecting = "{0..113, 115, 117..200}";
            string result = ( s.And( s2 ) ).ToString();
            assertEquals( result, expecting );
        }

    }
}
