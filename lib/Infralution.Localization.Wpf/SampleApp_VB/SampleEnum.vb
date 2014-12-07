#Region "File Header"
'
' COPYRIGHT:   Copyright 2009 
'              Infralution
'
#End Region
Imports System.ComponentModel
Imports Infralution.Localization.Wpf

''' <summary>
''' Sample enum illustrating used of a localized enum type converter
''' </summary>
<TypeConverter(GetType(SampleEnumConverter))> _
Public Enum SampleEnum
    VerySmall
    Small
    Medium
    Large
    VeryLarge
End Enum

''' <summary>
''' Define the type converter for the Sample Enum
''' </summary>
''' <remarks></remarks>
Class SampleEnumConverter
    Inherits ResourceEnumConverter

    ''' <summary>
    ''' Create a new instance of the converter using translations from the given resource manager
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        MyBase.New(GetType(SampleEnum), My.Resources.ResourceManager)
    End Sub
End Class
