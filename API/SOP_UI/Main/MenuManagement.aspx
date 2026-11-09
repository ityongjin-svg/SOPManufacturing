<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MenuManagement.aspx.cs" Inherits="SOP_UI.Main.MenuManagement" Async="true"%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .form-card {
            background: #fff;
            border-radius: 12px;
            padding: 20px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.08);
        }

        .btn-orange {
            background: linear-gradient(135deg, #ff7a18, #ffb347);
            border: none;
            color: white;
        }

        .btn-orange:hover {
            opacity: 0.9;
        }
    </style>
    <div class="container mt-4">

        <h3 class="mb-4">⚙️ Menu Management</h3>

        <div class="row">

            <!-- ================= FORM ================= -->
            <div class="col-md-4">

                <div class="form-card">

                    <h5 class="mb-3">📁 Menu Form</h5>

                    <asp:HiddenField ID="hfId" runat="server" />

                    <label class="form-label">Name</label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control mb-3" />

                    <label class="form-label">Parent</label>
                    <asp:DropDownList ID="ddlParent" runat="server" CssClass="form-control mb-3" />

                    <label class="form-label">URL</label>
                    <asp:TextBox ID="txtUrl" runat="server" CssClass="form-control mb-3" />

                    <div class="row">
                        <div class="col">
                            <label class="form-label">Level</label>
                            <asp:TextBox ID="txtLevel" runat="server" CssClass="form-control mb-3" />
                        </div>

                        <div class="col">
                            <label class="form-label">Sort</label>
                            <asp:TextBox ID="txtSort" runat="server" CssClass="form-control mb-3" />
                        </div>
                    </div>

                    <asp:Button ID="btnSave" runat="server"
                        Text="💾 Save Menu"
                        CssClass="btn btn-orange w-100"
                        OnClick="btnSave_Click" />

                </div>

            </div>

            <!-- ================= TABLE ================= -->
            <div class="col-md-8">

                <div class="form-card">

                    <h5 class="mb-3">📋 Menu List</h5>

                    <asp:GridView ID="gvMenu" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-hover table-striped align-middle"
                        OnRowCommand="gvMenu_RowCommand"
                        DataKeyNames="Id">

                        <Columns>

                            <asp:BoundField DataField="Id" HeaderText="ID" />

                            <asp:BoundField DataField="ParentName" HeaderText="Parent Name" />
                            
                            <asp:BoundField DataField="Name" HeaderText="Menu Name" />

                            <asp:BoundField DataField="ParentId" HeaderText="Parent" />

                            <asp:BoundField DataField="Level" HeaderText="Level" />

                            <asp:BoundField DataField="Url" HeaderText="URL" />

                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>

                                    <div class="d-flex gap-2">

                                        <asp:LinkButton runat="server"
                                            CommandName="editMenu"
                                            CommandArgument='<%# Eval("Id") %>'
                                            CssClass="btn btn-sm btn-primary">
                                            ✏️ Edit
                                        </asp:LinkButton>

                                        <asp:LinkButton runat="server"
                                            CommandName="deleteMenu"
                                            CommandArgument='<%# Eval("Id") %>'
                                            CssClass="btn btn-sm btn-danger"
                                            OnClientClick="return confirm('Delete this menu?');">
                                            🗑 Delete
                                        </asp:LinkButton>

                                    </div>

                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>

                    </asp:GridView>

                </div>

            </div>

        </div>

    </div>

</asp:Content>