<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ChatBotApp.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>login</title>
    <link rel="stylesheet" type="text/css" href="../App_Themes/style.css" />
    <style type="text/css">
        .gmailbutton {
            background-color: #ff0000;
            color: white;
            width: 150px;
            height: 30px;
        }
    </style>
</head>
<body style="background-image: url(../Images/background.jpg); background-size: cover; background-repeat: no-repeat;">
    <form id="form1" runat="server">
        <div id='bot-container'>
            <div id='chatborder'>
                <div style="text-align: center">
                    <img id="systemlogo" alt="Close" onclick="return false;" src="../Images/bot-mini.png"
                        style="width: 150px; height: 150px;" />
                </div>
                <p id="chatlog3" class="chatlog" style="text-align: center">Eagles Nest Chatbot</p>
                <p id="chatlog2" class="chatlog">&nbsp;</p>
                <p id="chatlog1" class="chatlog">&nbsp;</p>
                <div style="text-align: center">
                    <asp:Button ID="btnlogin" runat="server" Text="Login With Gmail" CssClass="gmailbutton" OnClick="btnlogin_Click" />
                </div>
                <div style="text-align: center; padding-top: 75px">
                    Created By <b>Rahul Razdan</b>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
