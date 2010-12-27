<%@ LANGUAGE = "VBScript" %>
<% 
Option Explicit

Const strRev = "$Revision: 12 $"

' -------------
' CONFIGURATION
Const strFiName = "マイクロソフト銀行"
Const strDbPath = "\\kkdfd1\public\wwwroot\AS\ActvStmt.mdb"
Dim fDebug
fDebug = (Request.QueryString("DEBUG") <> "")

' --------------------------------
' Establish connection to database
Dim adoCon, adoRsAcct
Set adoCon = AdoConOpen

' -------------------
' Prepare HTTP header

Response.ExpiresAbsolute = 0 ' Disable client-side cache
Response.ContentType = "application/x-ofx"
Response.Addheader "Content-Disposition", "filename=download.ofx"

' --------------------------------------------
' MAIN PROGRAM - WRITE OFX HEADER AND CONTENTS

Dim strBuf

' Write OFX header
strBuf = "OFXHEADER:100" & vbNewLine & _
	"DATA:OFXSGML" & vbNewLine & _
	"VERSION:102" & vbNewLine & _
	"SECURITY:NONE" & vbNewLine & _
	"ENCODING:UTF-8" & vbNewLine & _
	"CHARSET:CSUNICODE" & vbNewLine & _
	"COMPRESSION:NONE" & vbNewLine & _
	"OLDFILEUID:NONE" & vbNewLine & _
	"NEWFILEUID:NONE" & vbNewLine & vbNewLine
Response.Write strBuf

' Write OFX tags
WriteOFX

' END MAIN PROGRAM
' ----------------

' --------------------
' DATABASE CONNECTIONS

Function AdoConOpen()
	Dim cn
	Set cn = Server.CreateObject("ADODB.Connection")
	cn.Provider = "Microsoft.Jet.OLEDB.3.51"
	cn.Properties("Data Source") = strDbPath
	cn.Open
	Set AdoConOpen = cn
End Function

Function StrSqlBuildCriteria(strSql, strCriteria)
	If strSql = "" Then strSql = " WHERE " Else strSql = strSql & " AND "
	StrSqlBuildCriteria = strSql & strCriteria
End Function

Function StrSqlBuildCriteriaEq(strSql, strKey, strValue)
	If strValue <> "" Then strSql = StrSqlBuildCriteria(strSql, strKey & " = " & strValue)
	StrSqlBuildCriteriaEq = strSql
End Function

Function AdoRsOpenAccount(strAcctTypeCriteria)
	Dim strSql
	strSql = StrSqlBuildCriteriaEq("", "UID", Request.QueryString("UID"))
	strSql = StrSqlBuildCriteriaEq(strSql, "ACCTKEY", Request.QueryString("ACCTKEY"))
	If strAcctTypeCriteria <> "" Then strSql = StrSqlBuildCriteria(strSql, "ACCTTYPE" & strAcctTypeCriteria)
	Set AdoRsOpenAccount = adoCon.Execute("SELECT * FROM Account" & strSql)
End Function

' -------------------------------
' OUTPUT STRING IN UTF-8 ENCODING
Dim objCodeConv
Sub WriteUTF8(s)
	If IsEmpty(objCodeConv) Then Set objCodeConv = Server.CreateObject("Evita.Convert")
	Response.BinaryWrite objCodeConv.toByteVec(objCodeConv.toUTF8(s))
End Sub

' -------------------
' TAG-VALUE UTILITIES
Sub BlockBegin(s)
	Response.Write "<" & s & ">" & vbNewLine
End Sub

Sub BlockEnd(s)
	Response.Write "</" & s & ">" & vbNewLine
End Sub

Sub WriteTag(tag, strValue, fOptional, cchMax)
	' Trim string to appropriate length if specified
	strValue = Trim(strValue)
	If cchMax > 0 And Not fDebug Then strValue = Trim(Left(strValue, cchMax))
	If IsNull(strValue) Or strValue = "" Then
		If fOptional Then Exit Sub
		AssertAbort "invalid record"
	End If

	' Encode special characters like <, >, &
	strValue = Server.HTMLEncode(strValue)

	' Write in UTF-8
	WriteUTF8 "<" & tag & ">" & strValue & vbNewLine
End Sub

' --------------------------------
' Format date in OFX format in JST
Function StrFormatOFXDate(date)
	StrFormatOFXDate = Year(date)
	StrFormatOFXDate = StrFormatOFXDate & Right("00" & Month(date), 2)
	StrFormatOFXDate = StrFormatOFXDate & Right("00" & Day(date), 2)
	StrFormatOFXDate = StrFormatOFXDate & Right("00" & Hour(date), 2)
	StrFormatOFXDate = StrFormatOFXDate & Right("00" & Minute(date), 2)
	StrFormatOFXDate = StrFormatOFXDate & Right("00" & Second(date), 2)
	StrFormatOFXDate = StrFormatOFXDate & "[+9:JST]"
End Function

' ---------------
' DEBUG UTILITIES
Sub AssertAbort(s)
	If fDebug Then
		' Break into debugger
		STOP
	Else
		Response.Write "UNEXPECTED ERROR OCCURRED: " & s & vbNewLine
		Response.End
	End If
End Sub

' ----------------
' WRITE OFX BLOCKS

Dim adoRsTrn

Sub WriteOFX()
	BlockBegin "OFX"
	WriteSignOnMsgsRs

	' Write all banking accounts
	Set adoRsAcct = AdoRsOpenAccount("<> 'CREDITCARD'")
	If NOT adoRsAcct.EOF Then
		BlockBegin "BANKMSGSRSV1"
		Do While NOT adoRsAcct.EOF
			WriteStmtTrnRs
			adoRsAcct.MoveNext
		Loop
		BlockEnd "BANKMSGSRSV1"
	End If

	' Write all credit card accounts
	Set adoRsAcct = AdoRsOpenAccount("= 'CREDITCARD'")
	If NOT adoRsAcct.EOF Then
		BlockBegin "CREDITCARDMSGSRSV1"
		Do While NOT adoRsAcct.EOF
			WriteCCStmtTrnRs
			adoRsAcct.MoveNext
		Loop
		BlockEnd "CREDITCARDMSGSRSV1"
	End If

	BlockEnd "OFX"
End Sub

Sub WriteSignOnMsgsRs
	BlockBegin "SIGNONMSGSRSV1"
	BlockBegin "SONRS"
	WriteStatusOK
	WriteTag "DTSERVER", StrFormatOFXDate(Now), False, 0
	WriteTag "LANGUAGE", "JPN", False, 0
	If strFiName <> "" Then
		BlockBegin "FI"
		WriteTag "ORG", strFiName, False, 32
		BlockEnd "FI"
	End If
	BlockEnd "SONRS"
	BlockEnd "SIGNONMSGSRSV1"
End Sub

Sub WriteStatusOK
	Response.Write "<STATUS><CODE>0<SEVERITY>INFO</STATUS>" & vbNewLine
End Sub

' --------------------------------------
' STMTTRNRS -- bank statement main block
Sub WriteStmtTrnRs
	BlockBegin "STMTTRNRS"
	WriteTag "TRNUID", 0, False, 0
	WriteStatusOK
	WriteStmtRs "STMTRS"
	BlockEnd "STMTTRNRS"
End Sub

' -----------------------------------------------
' CCSTMTTRNRS -- credit card statement main block
Sub WriteCCStmtTrnRs
	BlockBegin "CCSTMTTRNRS"
	WriteTag "TRNUID", 0, False, 0
	WriteStatusOK
	WriteStmtRs "CCSTMTRS"
	BlockEnd "CCSTMTTRNRS"
End Sub

' ----------------------------------------------
' STMTRS/CCSTMTRS -- bank/credit card statements
Sub WriteStmtRs(sSTMTRS)
	BlockBegin sSTMTRS
	WriteTag "CURDEF", "JPY", False, 0
	WriteAcctFrom
	Set adoRsTrn = adoCon.Execute("SELECT * FROM Trans WHERE ACCTKEY=" & adoRsAcct("ACCTKEY") & " ORDER BY Date")
	If Not adoRsTrn.EOF Then
		BlockBegin "BANKTRANLIST"
		WriteTag "DTSTART", StrFormatOFXDate(adoRsTrn("Date")), False, 0
		WriteTag "DTEND", StrFormatOFXDate(Now), False, 0
		Do While NOT adoRsTrn.EOF
			WriteStmtTrn adoRsTrn("TRNTYPE"), adoRsTrn("Date"), _
				adoRsTrn("Amount"), adoRsTrn("FITID"), _
				adoRsTrn("NAME"), adoRsTrn("MEMO")
			adoRsTrn.MoveNext
		Loop
		BlockEnd "BANKTRANLIST"
	End If
	WriteBalance
	BlockEnd sSTMTRS
End Sub

' ------------------------------------------------
' ACCTFROM -- bank/credit card account information
Function StrConcatWithDelimiter(str1, str2, strDelimiter)
	If str1 <> "" And str2 <> "" Then
		StrConcatWithDelimiter = str1 & strDelimiter & str2
	Else
		StrConcatWithDelimiter = str1 & str2
	End If
End Function

Sub WriteAcctFrom
	Dim strAcctType
	strAcctType = adoRsAcct("ACCTTYPE")
	Select Case strAcctType
	Case "CHECKING", "SAVINGS", "MONEYMRKT", "CREDITLINE"
		BlockBegin "BANKACCTFROM"
		WriteTag "BANKID", adoRsAcct("FIID"), False, 9
		WriteTag "BRANCHID", adoRsAcct("BRANCHID"), True, 22
		WriteTag "ACCTID", adoRsAcct("ACCTID"), False, 22
		WriteTag "ACCTTYPE", strAcctType, False, 0
		BlockEnd "BANKACCTFROM"
	Case "CREDITCARD"
		BlockBegin "CCACCTFROM"
		Dim s
		s = StrConcatWithDelimiter(adoRsAcct("FIID"), adoRsAcct("BRANCHID"), "-")
		s = StrConcatWithDelimiter(s, adoRsAcct("ACCTID"), "-")
		WriteTag "ACCTID", s, False, 22
		BlockEnd "CCACCTFROM"
	Case Else
		AssertAbort "invalid account type in the record"
	End Select
End Sub

Sub WriteStmtTrn(strTrnType, dtPosted, aTrnAmt, strFitId, strName, strMemo)
	' TRNTYPE cannot be omitted, if not in database, fill in
	if IsEmpty(strTrnType) Or IsNull(strTrnType) Or strTrnType = "" Then strTrnType = "OTHER"
	BlockBegin "STMTTRN"
	WriteTag "TRNTYPE", strTrnType, False, 0
	WriteTag "DTPOSTED", StrFormatOFXDate(dtPosted), False, 0
	WriteTag "TRNAMT", aTrnAmt, False, 0
	If IsNull(strFitId) Or strFitId = "" Then strFitId = 0
	WriteTag "FITID", strFitId, False, 255
	WriteTag "NAME", strName, True, 32
	WriteTag "MEMO", strMemo, True, 255
	BlockEnd "STMTTRN"
End Sub

' -------------------------------------
' LEDGARBAL -- bank/credit card balance
Sub WriteBalance
	BlockBegin "LEDGERBAL"
	WriteTag "BALAMT", adoRsAcct("BALANCE"), False, 0
	WriteTag "DTASOF", StrFormatOFXDate(adoRsAcct("BALASOF")), False, 0
	BlockEnd "LEDGERBAL"
	If Not IsNull(adoRsAcct("AVAIL")) Then
		BlockBegin "AVAILBAL"
		WriteTag "BALAMT", adoRsAcct("AVAIL"), False, 0
		WriteTag "DTASOF", StrFormatOFXDate(adoRsAcct("AVAILASOF")), False, 0
		BlockEnd "AVAILBAL"
	End If
End Sub
%>
