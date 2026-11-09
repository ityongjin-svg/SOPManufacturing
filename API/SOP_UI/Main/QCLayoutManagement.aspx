<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="QCLayoutManagement.aspx.cs" Inherits="SOP_UI.Main.QCLayoutManagement" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .form-card {
            background: #fff;
            padding: 20px;
            border-radius: 12px;
            box-shadow: 0 5px 20px rgba(0,0,0,0.08);
        }
        #uploadOverlay.show {
            display: flex !important;
        }
        .qc-mgmt-filters {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
        }
        .qc-mgmt-filters input {
            max-width: 220px;
        }
    </style>

    <asp:HiddenField ID="hfHasProcessing" runat="server" />
    <div class="container mt-4">
        <h3 class="mb-4">🎬 QC Inspection Layout Management</h3>

        <div class="form-card mb-4">
            <asp:DropDownList ID="ddlBuyer" runat="server" CssClass="form-select mb-3" />
            <asp:TextBox ID="txtStyleName" runat="server" CssClass="form-control mb-3" placeholder="Style Name..." />
            <asp:DropDownList ID="ddlItem" runat="server" CssClass="form-select mb-3" />
            <asp:DropDownList ID="ddlSort" runat="server" CssClass="form-select mb-3" />
            <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control mb-3" TextMode="MultiLine" Rows="4" placeholder="Description..." />

            <label class="form-label">Video</label>
            <asp:FileUpload ID="fuVideo" runat="server" CssClass="form-control mb-1" />
            <asp:Label ID="lblCurrentVideo" runat="server" CssClass="text-muted d-block mb-3" />

            <label class="form-label">File</label>
            <asp:FileUpload ID="fuFile" runat="server" CssClass="form-control mb-1" />
            <asp:Label ID="lblCurrentFile" runat="server" CssClass="text-muted d-block mb-3" />

            <asp:Button ID="btnSave" runat="server" Text="Save Style" CssClass="btn btn-success w-100" 
                OnClick="btnSave_Click" OnClientClick="showUploadSpinner();" />

            <asp:Label ID="lblMsg" runat="server" CssClass="text-danger d-block mt-2" />
        </div>

        <div class="qc-mgmt-filters mb-3" id="filterBar" runat="server"></div>

        <asp:GridView ID="gvStyle" runat="server"
            AutoGenerateColumns="false"
            CssClass="table table-hover table-striped"
            DataKeyNames="Id"
            OnRowCommand="gvStyle_RowCommand"
            OnRowDataBound="gvStyle_RowDataBound">
            <Columns>
                <asp:BoundField DataField="Buyer" HeaderText="Buyer Name" />
                <asp:BoundField DataField="StyleName" HeaderText="Style Name" />
                <asp:BoundField DataField="Item" HeaderText="Item Name" />
                <asp:BoundField DataField="Sort" HeaderText="Sort Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" />

                <asp:TemplateField HeaderText="Video">
                    <ItemTemplate>
                        <%# (bool)Eval("HasVideo") ? "✅" : "—" %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="File">
                    <ItemTemplate>
                        <%# (bool)Eval("HasFile") ? Eval("FileFileName") : "—" %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:ButtonField Text="Edit" CommandName="editBaris" />

                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkDelete" runat="server"
                            CommandName="deleteBaris"
                            CommandArgument='<%# ((GridViewRow)Container).RowIndex %>'
                            OnClientClick="return confirm('Yakin mau hapus style ini? Data yang sudah dihapus tidak bisa dikembalikan.');"
                            CssClass="btn btn-sm btn-outline-danger">
                            Delete
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <%# Eval("Status").ToString() == "Processing" 
                              ? "<span class='badge bg-warning text-dark'>⏳ Processing</span>" 
                            : Eval("Status").ToString() == "Failed"
                              ? "<span class='badge bg-danger'>❌ Failed</span>"
                              : "<span class='badge bg-success'>✅ Ready</span>" %>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
    <div id="uploadOverlay" style="display:none; position:fixed; top:0; left:0; width:100%; height:100%; 
         background:rgba(15,23,42,0.75); z-index:9999; align-items:center; justify-content:center; flex-direction:column;">
        <div class="spinner-border text-light" style="width:3.5rem; height:3.5rem;" role="status"></div>
        <div style="color:white; margin-top:16px; font-size:15px; font-weight:600;">
            Mengunggah &amp; memproses video, mohon tunggu...
        </div>
    </div>

    <script>
        function showUploadSpinner() {
            // hanya munculkan spinner kalau memang ada file video yang dipilih,
            // supaya tidak mengganggu saat cuma edit teks tanpa upload video baru
            var overlay = document.getElementById("uploadOverlay");
            overlay.classList.add("show");
        }
        function filterGrid() {
            function val(id) {
                var el = document.getElementById(id);
                return el ? el.value.toLowerCase() : "";
            }

            let fBuyer = val("filterBuyer");
            let fStyle = val("filterStyle");
            let fItem = val("filterItem");
            let fSort = val("filterSort");

            document.querySelectorAll("#<%= gvStyle.ClientID %> tr").forEach(function (row) {
                if (!row.hasAttribute("data-buyer")) return; // lewati baris header

                let match = (!fBuyer || row.getAttribute("data-buyer").toLowerCase().includes(fBuyer))
                    && (!fStyle || row.getAttribute("data-style").toLowerCase().includes(fStyle))
                    && (!fItem || row.getAttribute("data-item").toLowerCase().includes(fItem))
                    && (!fSort || row.getAttribute("data-sort").toLowerCase().includes(fSort));

                row.style.display = match ? "" : "none";
            });
        }
    </script>
</asp:Content>
