<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="ChatBotApp.Index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="../App_Themes/BotStyle.css" />
    <script src="https://code.jquery.com/jquery-3.6.1.min.js" type="text/javascript"></script>
</head>
<body style="background-image: url(../Images/background.jpg); background-size: cover; background-repeat: no-repeat;">
    <form id="form1" runat="server">
        <script type="text/javascript" src='https://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.8.3.min.js'></script>
        <script type="text/javascript" src='https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.0.3/js/bootstrap.min.js'></script>
        <link rel="stylesheet" href='https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.0.3/css/bootstrap.min.css'
            media="screen" />
        <script type="text/javascript">
            function ShowPopup(title) {
                $("#MyPopup .modal-title").html(title);
                $("#MyPopup").modal("show");
            }
        </script>
        <asp:ScriptManager runat="server" ID="scriptMgr"></asp:ScriptManager>
        <asp:UpdatePanel runat="server" ID="upPanel">
            <ContentTemplate>
                <asp:Panel runat="server" ID="Panel1" DefaultButton="botsendbutton">
                    <div id="bot-container" class="">
                        <div id="bot-title-grid">
                            <div class="bot-title-avatar"></div>
                            <div class="bot-title-text">Eddie</div>
                            <asp:Button ID="btnLogout" runat="server" OnClick="btnLogout_Click" Text="Logout" Width="100px" />
                        </div>
                        <div id="bot-message-grid">
                            <ul id="botmessagelist" class="bot-message-list" runat="server"></ul>
                            <img id="imageBtn" src="Images/close-button.png" title="hello" style="display: none;" />
                        </div>
                        <div id="bot-input-grid">
                            <input id="botinputbox" runat="server" type="text" placeholder="Type here..." style="text-transform: uppercase" />
                            <asp:Button ID="botsendbutton" runat="server" OnClick="btnSend_Click" />
                        </div>
                        <!-- Message Template -->
                        <div class="message-template" style="display: none">
                            <li class="message">
                                <div class="message-avatar"></div>
                                <div class="message-wrapper">
                                    <div class="message-text"></div>
                                </div>
                            </li>
                        </div>
                        <div class="indicator-template" style="display: none">
                            <div class="bot-typing-indicator">
                                <span></span>
                                <span></span>
                                <span></span>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        <asp:Button ID="botclearchatbutton" runat="server" OnClick="btnClearChat_Click" Text="Clear Chat" />
        <asp:Button ID="botaddChatbutton" runat="server" OnClick="btnStartChat_Click" Text="Start Chat" />
        <asp:Button ID="botprofilebutton" runat="server" OnClick="btnProfile_Click" Text="Profile" />
        <div id="MyPopup" class="modal fade" role="dialog">
            <div class="modal-dialog">
                <!-- Modal content-->
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">
                            &times;</button>
                        <h4 class="modal-title"></h4>
                    </div>
                    <div class="modal-body">
                        <table style="width: 100%">
                            <tr>
                                <td colspan="2">
                                    <asp:Image ID="imgprofile" runat="server" Height="100px" Width="100px" />
                                </td>
                            </tr>
                            <tr>
                                <td>Id
                                </td>
                                <td>
                                    <asp:Label ID="lblid" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>Name
                                </td>
                                <td>
                                    <asp:Label ID="lblname" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>Gender
                                </td>
                                <td>
                                    <asp:Label ID="lblgender" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>locale
                                </td>
                                <td>
                                    <asp:Label ID="lbllocale" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>Email
                                </td>
                                <td>
                                    <asp:Label ID="lblEmail" runat="server">Profile link</asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>A1</td>
                                <td>
                                    <asp:TextBox ID="txtA1" runat="server" Text="" Width="400px" />
                                </td>
                            </tr>
                            <tr>
                                <td>A2</td>
                                <td>
                                    <asp:TextBox ID="txtA2" runat="server" Text="" Width="400px" /></td>
                            </tr>
                            <tr>
                                <td>A3</td>
                                <td>
                                    <asp:TextBox ID="txtA3" runat="server" Text="" Width="400px" /></td>
                            </tr>
                            <tr>
                                <td>A4</td>
                                <td>
                                    <asp:TextBox ID="txtA4" runat="server" Text="" Width="400px" /></td>
                            </tr>
                            <tr>
                                <td>B1</td>
                                <td>
                                    <asp:TextBox ID="txtB1" runat="server" Text="" Width="400px" /></td>
                            </tr>
                            <tr>
                                <td>B2</td>
                                <td>
                                    <asp:TextBox ID="txtB2" runat="server" Text="" Width="400px" /></td>
                            </tr>
                            <tr>
                                <td>B3</td>
                                <td>
                                    <asp:TextBox ID="txtB3" runat="server" Text="" Width="400px" /></td>
                            </tr>
                            <tr>
                                <td>B4</td>
                                <td>
                                    <asp:TextBox ID="txtB4" runat="server" Text="" Width="400px" /></td>
                            </tr>
                        </table>
                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="btnSaveClose" runat="server" Text="Save & Close" OnClick="SaveClose" CssClass="btn btn-danger" />
                        <button type="button" class="btn btn-danger" data-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <script>
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_beginRequest(BeginRequestHandler);
        prm.add_endRequest(EndRequestHandler);

        function BeginRequestHandler(sender, args) {
        }

        function EndRequestHandler(sender, args) {
            $('#bot-message-grid').animate({ scrollTop: $('#bot-message-grid')[0].scrollHeight }, "slow");
        }
    </script>
</body>
</html>
