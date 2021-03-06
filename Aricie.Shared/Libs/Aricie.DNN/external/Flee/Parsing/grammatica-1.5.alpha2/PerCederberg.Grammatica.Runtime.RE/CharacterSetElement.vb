' 
' * CharacterSetElement.cs 
' * 
' * This library is free software; you can redistribute it and/or 
' * modify it under the terms of the GNU Lesser General Public License 
' * as published by the Free Software Foundation; either version 2.1 
' * of the License, or (at your option) any later version. 
' * 
' * This library is distributed in the hope that it will be useful, 
' * but WITHOUT ANY WARRANTY; without even the implied warranty of 
' * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
' * Lesser General Public License for more details. 
' * 
' * You should have received a copy of the GNU Lesser General Public 
' * License along with this library; if not, write to the Free 
' * Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, 
' * MA 02111-1307, USA. 
' * 
' * Copyright (c) 2003-2005 Per Cederberg. All rights reserved. 
' 

' Converted to VB.NET	[Eugene Ciloci; Nov 24, 2007]

Imports System 
Imports System.Collections 
Imports System.IO 
Imports System.Text 

Imports Ciloci.Flee.PerCederberg.Grammatica.Runtime

Namespace PerCederberg.Grammatica.Runtime.RE 
    
    '* 
' * A regular expression character set element. This element 
' * matches a single character inside (or outside) a character set. 
' * The character set is user defined and may contain ranges of 
' * characters. The set may also be inverted, meaning that only 
' * characters not inside the set will be considered to match. 
' * 
' * @author Per Cederberg, <per at percederberg dot net> 
' * @version 1.5 
' 
    
    Friend Class CharacterSetElement 
        Inherits Element 
        
        '* 
' * The dot ('.') character set. This element matches a single 
' * character that is not equal to a newline character. 
' 
        
        Public Shared DOT As New CharacterSetElement(False) 
        
        '* 
' * The digit character set. This element matches a single 
' * numeric character. 
' 
        
        Public Shared DIGIT As New CharacterSetElement(False) 
        
        '* 
' * The non-digit character set. This element matches a single 
' * non-numeric character. 
' 
        
        Public Shared NON_DIGIT As New CharacterSetElement(True) 
        
        '* 
' * The whitespace character set. This element matches a single 
' * whitespace character. 
' 
        
        Public Shared WHITESPACE As New CharacterSetElement(False) 
        
        '* 
' * The non-whitespace character set. This element matches a 
' * single non-whitespace character. 
' 
        
        Public Shared NON_WHITESPACE As New CharacterSetElement(True) 
        
        '* 
' * The word character set. This element matches a single word 
' * character. 
' 
        
        Public Shared WORD As New CharacterSetElement(False) 
        
        '* 
' * The non-word character set. This element matches a single 
' * non-word character. 
' 
        
        Public Shared NON_WORD As New CharacterSetElement(True) 
        
        '* 
' * The inverted character set flag. 
' 
        
        Private inverted As Boolean 
        
        '* 
' * The character set content. This array may contain either 
' * range objects or Character objects. 
' 
        
        Private contents As New ArrayList() 
        
        '* 
' * Creates a new character set element. If the inverted character 
' * set flag is set, only characters NOT in the set will match. 
' * 
' * @param inverted the inverted character set flag 
' 
        
        Public Sub New(ByVal inverted As Boolean) 
            Me.inverted = inverted 
        End Sub 
        
        '* 
' * Adds a single character to this character set. 
' * 
' * @param c the character to add 
' 
        
        Public Sub AddCharacter(ByVal c As Char) 
            contents.Add(c) 
        End Sub 
        
        '* 
' * Adds multiple characters to this character set. 
' * 
' * @param str the string with characters to add 
' 
        
        Public Sub AddCharacters(ByVal str As String) 
            For i As Integer = 0 To str.Length - 1 
                AddCharacter(str(i)) 
            Next 
        End Sub 
        
        '* 
' * Adds multiple characters to this character set. 
' * 
' * @param elem the string element with characters to add 
' 
        
        Public Sub AddCharacters(ByVal elem As StringElement) 
            AddCharacters(elem.GetString()) 
        End Sub 
        
        '* 
' * Adds a character range to this character set. 
' * 
' * @param min the minimum character value 
' * @param max the maximum character value 
' 
        
        Public Sub AddRange(ByVal min As Char, ByVal max As Char) 
            contents.Add(New Range(min, max)) 
        End Sub 
        
        '* 
' * Adds a character subset to this character set. 
' * 
' * @param elem the character set to add 
' 
        
        Public Sub AddCharacterSet(ByVal elem As CharacterSetElement) 
            contents.Add(elem) 
        End Sub 
        
        '* 
' * Returns this element as the character set shouldn't be 
' * modified after creation. This partially breaks the contract 
' * of clone(), but as new characters are not added to the 
' * character set after creation, this will work correctly. 
' * 
' * @return this character set element 
' 
        
        Public Overloads Overrides Function Clone() As Object 
            Return Me 
        End Function 
        
        '* 
' * Returns the length of a matching string starting at the 
' * specified position. The number of matches to skip can also be 
' * specified, but numbers higher than zero (0) cause a failed 
' * match for any element that doesn't attempt to combine other 
' * elements. 
' * 
' * @param m the matcher being used 
' * @param input the input character stream to match 
' * @param start the starting position 
' * @param skip the number of matches to skip 
' * 
' * @return the length of the matching string, or 
' * -1 if no match was found 
' * 
' * @throws IOException if an I/O error occurred 
' 
        
        Public Overloads Overrides Function Match(ByVal m As Matcher, ByVal input As LookAheadReader, ByVal start As Integer, ByVal skip As Integer) As Integer 
            
            Dim c As Integer 
            
            If skip <> 0 Then 
                Return -1 
            End If 
            c = input.Peek(start) 
            If c < 0 Then 
                m.SetReadEndOfString() 
                Return -1 
            End If 
            If m.IsCaseInsensitive() Then 
				c = Convert.ToInt32([Char].ToLower(Convert.ToChar(c)))
            End If 
			Return IIf(InSet(Convert.ToChar(c)), 1, -1)
        End Function 
        
        '* 
' * Checks if the specified character matches this character 
' * set. This method takes the inverted flag into account. 
' * 
' * @param c the character to check 
' * 
' * @return true if the character matches, or 
' * false otherwise 
' 
        
        Private Function InSet(ByVal c As Char) As Boolean 
			If Me Is DOT Then
				Return InDotSet(c)
			ElseIf Me Is DIGIT OrElse Me Is NON_DIGIT Then
				Return InDigitSet(c) <> inverted
			ElseIf Me Is WHITESPACE OrElse Me Is NON_WHITESPACE Then
				Return InWhitespaceSet(c) <> inverted
			ElseIf Me Is WORD OrElse Me Is NON_WORD Then
				Return InWordSet(c) <> inverted
			Else
				Return InUserSet(c) <> inverted
			End If
        End Function 
        
        '* 
' * Checks if the specified character is present in the 'dot' 
' * set. This method does not consider the inverted flag. 
' * 
' * @param c the character to check 
' * 
' * @return true if the character is present, or 
' * false otherwise 
' 
        
        Private Function InDotSet(ByVal c As Char) As Boolean 
            Select Case c 
				Case ChrW(10), ChrW(13), ChrW(133), ChrW(&H2028), ChrW(&H2029)
					Return False
                Case Else 
                    Return True 
            End Select 
        End Function 
        
        '* 
' * Checks if the specified character is a digit. This method 
' * does not consider the inverted flag. 
' * 
' * @param c the character to check 
' * 
' * @return true if the character is a digit, or 
' * false otherwise 
' 
        
        Private Function InDigitSet(ByVal c As Char) As Boolean 
            Return "0"C <= c AndAlso c <= "9"C 
        End Function 
        
        '* 
' * Checks if the specified character is a whitespace 
' * character. This method does not consider the inverted flag. 
' * 
' * @param c the character to check 
' * 
' * @return true if the character is a whitespace character, or 
' * false otherwise 
' 
        
        Private Function InWhitespaceSet(ByVal c As Char) As Boolean 
            Select Case c 
				Case " "c, Chr(9), Chr(10), Chr(12), Chr(13), Chr(11)
					Return True
                Case Else 
                    Return False 
            End Select 
        End Function 
        
        '* 
' * Checks if the specified character is a word character. This 
' * method does not consider the inverted flag. 
' * 
' * @param c the character to check 
' * 
' * @return true if the character is a word character, or 
' * false otherwise 
' 
        
        Private Function InWordSet(ByVal c As Char) As Boolean 
            Return ("a"C <= c AndAlso c <= "z"C) OrElse ("A"C <= c AndAlso c <= "Z"C) OrElse ("0"C <= c AndAlso c <= "9"C) OrElse c = "_"C 
        End Function 
        
        '* 
' * Checks if the specified character is present in the user- 
' * defined set. This method does not consider the inverted 
' * flag. 
' * 
' * @param value the character to check 
' * 
' * @return true if the character is present, or 
' * false otherwise 
' 
        
        Private Function InUserSet(ByVal value As Char) As Boolean 
            Dim obj As Object 
            Dim c As Char 
            Dim r As Range 
            Dim e As CharacterSetElement 
            For i As Integer = 0 To contents.Count - 1 
                
                obj = contents(i) 
                If TypeOf obj Is Char Then 
                    c = CChar(obj) 
                    If c = value Then 
                        Return True 
                    End If 
ElseIf TypeOf obj Is Range Then 
                    r = DirectCast(obj, Range) 
                    If r.Inside(value) Then 
                        Return True 
                    End If 
ElseIf TypeOf obj Is CharacterSetElement Then 
                    e = DirectCast(obj, CharacterSetElement) 
                    If e.InSet(value) Then 
                        Return True 
                    End If 
                End If 
            Next 
            Return False 
        End Function 
        
        '* 
' * Prints this element to the specified output stream. 
' * 
' * @param output the output stream to use 
' * @param indent the current indentation 
' 
        
        Public Overloads Overrides Sub PrintTo(ByVal output As TextWriter, ByVal indent As String) 
            output.WriteLine(indent + ToString()) 
        End Sub 
        
        '* 
' * Returns a string description of this character set. 
' * 
' * @return a string description of this character set 
' 
        
        Public Overloads Overrides Function ToString() As String 
            Dim buffer As StringBuilder 
            
            ' Handle predefined character sets 
			If Me Is DOT Then
				Return "."
			ElseIf Me Is DIGIT Then
				Return "\d"
			ElseIf Me Is NON_DIGIT Then
				Return "\D"
			ElseIf Me Is WHITESPACE Then
				Return "\s"
			ElseIf Me Is NON_WHITESPACE Then
				Return "\S"
			ElseIf Me Is WORD Then
				Return "\w"
			ElseIf Me Is NON_WORD Then
				Return "\W"
			End If
            
            ' Handle user-defined character sets 
            buffer = New StringBuilder() 
            If inverted Then 
                buffer.Append("^[") 
            Else 
                buffer.Append("[") 
            End If 
            For i As Integer = 0 To contents.Count - 1 
                buffer.Append(contents(i)) 
            Next 
            buffer.Append("]") 
            
            Return buffer.ToString() 
        End Function 
        
        
        '* 
' * A character range class. 
' 
        
        Private Class Range 
            
            '* 
' * The minimum character value. 
' 
            
            Private min As Char 
            
            '* 
' * The maximum character value. 
' 
            
            Private max As Char 
            
            '* 
' * Creates a new character range. 
' * 
' * @param min the minimum character value 
' * @param max the maximum character value 
' 
            
            Public Sub New(ByVal min As Char, ByVal max As Char) 
                Me.min = min 
                Me.max = max 
            End Sub 
            
            '* 
' * Checks if the specified character is inside the range. 
' * 
' * @param c the character to check 
' * 
' * @return true if the character is in the range, or 
' * false otherwise 
' 
            
            Public Function Inside(ByVal c As Char) As Boolean 
                Return min <= c AndAlso c <= max 
            End Function 
            
            '* 
' * Returns a string representation of this object. 
' * 
' * @return a string representation of this object 
' 
            
            Public Overloads Overrides Function ToString() As String 
                Return min + "-" + max 
            End Function 
        End Class 
    End Class 
End Namespace 