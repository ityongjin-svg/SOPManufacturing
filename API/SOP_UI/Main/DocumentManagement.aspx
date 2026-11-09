<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DocumentManagement.aspx.cs" Inherits="SOP_UI.Main.DocumentManagement" Async="true" %>

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

        <h3 class="mb-4">📄 Document Management</h3>

        <div class="row">

            <!-- ================= FORM ================= -->
            <div class="col-md-4">

                <div class="form-card">

                    <h5 class="mb-3">📤 Upload Document</h5>

                    <asp:HiddenField ID="hfId" runat="server" />

                    <label class="form-label">Title</label>
                    <asp:TextBox ID="txtTitle" runat="server"
                        CssClass="form-control mb-3"
                        placeholder="Document Title..." />

                    <label class="form-label">Menu</label>
                    <asp:DropDownList ID="ddlMenu" runat="server"
                        CssClass="form-control mb-3" />

                    <label class="form-label">Category</label>
                    <asp:DropDownList ID="ddlCategory" runat="server"
                        CssClass="form-control mb-3" />

                    <label class="form-label">Upload File</label>
                    <asp:FileUpload ID="fileUpload" runat="server"
                        CssClass="form-control mb-3" />

                    <asp:Button ID="btnSave" runat="server"
                        Text="💾 Save Document"
                        CssClass="btn btn-orange w-100"
                        OnClick="btnSave_Click" />

                </div>

            </div>

            <!-- ================= LIST ================= -->
            <div class="col-md-8">

                <div class="form-card">

                    <h5 class="mb-3">📋 Document List</h5>

                    <input type="text" id="docSearchBox" class="form-control mb-3"
                        placeholder="🔍 Cari judul, menu, kategori, atau tipe file..."
                        oninput="filterDocuments()" />

                    <asp:GridView ID="gvDocs" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-hover table-striped align-middle"
                        OnRowCommand="gvDocs_RowCommand"
                        DataKeyNames="Id">

                        <Columns>

                            <asp:BoundField DataField="Title" HeaderText="Title" />

                            <asp:BoundField DataField="MenuName" HeaderText="Menu" />

                            <asp:BoundField DataField="CategoryName" HeaderText="Category" />

                            <asp:BoundField DataField="FileType" HeaderText="Type" />

                            <asp:TemplateField HeaderText="Preview">
                                <ItemTemplate>
                                    <a href='<%# GetPreviewUrl(Eval("Id")) %>' target="_blank"
                                       class="btn btn-sm btn-outline-primary">
                                        🔍 Open
                                    </a>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>

                                    <div class="d-flex gap-2">

                                        <asp:LinkButton runat="server"
                                            CommandName="editBaris"
                                            CommandArgument='<%# Eval("Id") %>'
                                            CssClass="btn btn-sm btn-primary">
                                            ✏️ Edit
                                        </asp:LinkButton>

                                        <asp:LinkButton runat="server"
                                            CommandName="deleteBaris"
                                            CommandArgument='<%# Eval("Id") %>'
                                            CssClass="btn btn-sm btn-danger"
                                            OnClientClick="return confirm('Delete this document?');">
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
        <script>
        function filterDocuments() {
            let input = document.getElementById("docSearchBox").value.toLowerCase();
            let table = document.getElementById("<%= gvDocs.ClientID %>");
            let anyVisible = false;

            table.querySelectorAll("tr").forEach(function (row) {
                if (row.querySelector("th")) return; // lewati baris header

                let text = row.innerText.toLowerCase();
                let match = text.includes(input);
                row.style.display = match ? "" : "none";
                if (match) anyVisible = true;
            });

            let emptyMsg = document.getElementById("docEmptySearch");
            if (!anyVisible) {
                if (!emptyMsg) {
                    emptyMsg = document.createElement("div");
                    emptyMsg.id = "docEmptySearch";
                    emptyMsg.className = "text-center text-muted py-3";
                    emptyMsg.innerText = "🔍 Dokumen tidak ditemukan";
                    table.parentNode.appendChild(emptyMsg);
                }
            } else if (emptyMsg) {
                emptyMsg.remove();
            }
        }
        </script>
</asp:Content>
