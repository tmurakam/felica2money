object MForm: TMForm
  Left = 233
  Top = 123
  BorderStyle = bsSingle
  Caption = 'MoneyImport'
  ClientHeight = 230
  ClientWidth = 420
  Color = clBtnFace
  Font.Charset = SHIFTJIS_CHARSET
  Font.Color = clWindowText
  Font.Height = -12
  Font.Name = #65325#65331' '#65328#12468#12471#12483#12463
  Font.Style = []
  OldCreateOrder = False
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 12
  object Label1: TLabel
    Left = 32
    Top = 16
    Width = 48
    Height = 12
    Caption = #21475#24231#24773#22577
  end
  object ButtonConvert: TButton
    Left = 120
    Top = 184
    Width = 89
    Height = 25
    Caption = #22793#25563
    Font.Charset = SHIFTJIS_CHARSET
    Font.Color = clWindowText
    Font.Height = -12
    Font.Name = #65325#65331' '#65328#12468#12471#12483#12463
    Font.Style = [fsBold]
    ParentFont = False
    TabOrder = 0
    OnClick = ButtonConvertClick
  end
  object ButtonQuit: TButton
    Left = 224
    Top = 184
    Width = 49
    Height = 25
    Caption = #32066#20102
    TabOrder = 1
    OnClick = ButtonQuitClick
  end
  object ButtonHelp: TButton
    Left = 280
    Top = 184
    Width = 49
    Height = 25
    Caption = #12504#12523#12503
    TabOrder = 2
    OnClick = ButtonHelpClick
  end
  object AcGrid: TStringGrid
    Left = 24
    Top = 40
    Width = 369
    Height = 129
    ColCount = 4
    FixedCols = 2
    Options = [goFixedVertLine, goFixedHorzLine, goVertLine, goHorzLine, goRangeSelect, goEditing]
    TabOrder = 3
    OnExit = AcGridExit
    ColWidths = (
      104
      61
      62
      132)
    RowHeights = (
      24
      24
      24
      24
      24)
  end
  object OpenDialog: TOpenDialog
    Filter = 'CSV Files (*.csv)|*.csv|All Files (*.*)|*.*'
    Left = 80
    Top = 184
  end
end
