<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MachineManagement.aspx.cs" Inherits="SOP_UI.Main.MachineManagement" Async ="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        body {
            background: radial-gradient(circle at top left, #e6f8ff, #dae6f2 60%);
        }

        .machine-wrapper {
            max-width: 1100px;
            margin: auto;
        }

        .header-title {
            font-size: 26px;
            font-weight: 700;
            margin-bottom: 15px;
        }

        .card-box {
            background: white;
            padding: 20px;
            border-radius: 14px;
            margin-top: 15px;
            box-shadow: 0 5px 25px rgba(0,0,0,0.08);
        }

        .btn-main {
            background: #0ea5e9;
            color: white;
            border: none;
            padding: 8px 14px;
            border-radius: 10px;
        }

        .item-title {
            font-weight: 700;
            font-size: 16px;
        }

        .type-title {
            margin-left: 15px;
            color: #0ea5e9;
        }

        .file-row {
            margin-left: 30px;
            display: flex;
            justify-content: space-between;
            padding: 6px 0;
        }

        .btn-small {
            padding: 3px 8px;
            font-size: 12px;
            margin-left: 5px;
        }

        .machine-tabs {
            display: flex;
            gap: 10px;
        }

        .tab {
            padding: 10px 20px;
            border-radius: 10px;
            background: #f1f5f9;
            cursor: pointer;
            font-weight: 600;
        }

        .tab.active {
            background: linear-gradient(135deg, #0f172a, #0ea5e9);
            color: white;
        }
        /* GRID */
        .grid {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
            overflow: hidden;
            border-radius: 12px;
        }

        .grid th {
            background: linear-gradient(135deg, #0f172a, #0ea5e9);
            color: white;
            padding: 12px;
            text-align: left;
            font-size: 14px;
        }

        .grid td {
            padding: 10px 12px;
            border-bottom: 1px solid #e5e7eb;
            font-size: 14px;
        }

        .grid tr:hover {
            background: #f1f5f9;
        }

        /* BUTTON */
        .btn-action {
            padding: 4px 10px;
            border-radius: 6px;
            font-size: 12px;
            border: none;
            cursor: pointer;
        }

        .btn-edit {
            background: #f59e0b;
            color: white;
        }

        .btn-delete {
            background: #ef4444;
            color: white;
        }

        .btn-edit:hover {
            background: #d97706;
        }

        .btn-delete:hover {
            background: #dc2626;
        }

        /* SEARCH */
        .search-box {
            margin-top: 15px;
            margin-bottom: 10px;
        }

        .search-box input {
            width: 300px;
            padding: 8px 12px;
            border-radius: 10px;
            border: 1px solid #cbd5f5;
        }

        .btn-view {
            background: #10b981;
            color: white;
        }

        .btn-view:hover {
            background: #059669;
        }

        /*PopUp Modal*/
        .modal-bg {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0,0,0,0.4);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 999;
        }

        .modal-box {
            background: white;
            padding: 20px;
            border-radius: 12px;
            width: 800px;
            max-height: 80vh;
            display: flex;
            flex-direction: column;
            overflow: hidden; 
        }

        .modal-scroll-area {
            overflow-y: auto;
            flex: 1;
            margin-top: 10px;
        }

        /* GRID MASTER MACHINE */
        .grid-master {
            width: 100%;
            border-collapse: collapse;
            margin-top: 10px;
            font-size: 14px;
        }

        .grid-master th {
            background: #0f172a;
            color: white;
            padding: 12px;
            text-align: left;
        }

        .grid-master td {
            padding: 12px;
            border-bottom: 1px solid #e5e7eb;
            background: #fff;
        }

        .grid-master tr:hover td {
            background: #f1f5f9;
            transition: 0.2s;
        }

        /* BUTTON ACTION */
        .btn-view {
            padding: 5px 10px;
            border-radius: 8px;
            background: #10b981;
            color: white;
            text-decoration: none;
            font-size: 12px;
        }

        .btn-view:hover {
            background: #059669;
        }

        .btn-delete {
            padding: 5px 10px;
            border-radius: 8px;
            background: #ef4444;
            color: white;
            border: none;
            font-size: 12px;
            cursor: pointer;
        }

        .btn-delete:hover {
            background: #dc2626;
        }

        .modal-dialog {
            width: 90%;
            max-width: 1200px;
        }
        .modal-content {
            max-height: 700px;
            height: 50%;
        }
        .modal-body {
            height: calc(90vh - 60px);
            padding: 0;
        }
        #previewFrame {
            width: 100%;
            height: 100%;
            border: none;
        }
    </style>

    <div class="machine-wrapper">

        <div class="header-title">Machine Management</div>

        <!-- TAB -->
        <div class="machine-tabs">
             <div class='tab <%= (Request.QueryString["categoryId"] ?? "12") == "12" ? "active" : "" %>'
                 onclick="location.href='?categoryId=12'">Master Machine</div>

            <div class='tab <%= (Request.QueryString["categoryId"] == "10") ? "active" : "" %>'
                 onclick="location.href='?categoryId=10'">Assist Machine</div>

            <div class='tab <%= Request.QueryString["categoryId"] == "11" ? "active" : "" %>'
                 onclick="location.href='?categoryId=11'">Sewing Machine</div>
        </div>

        <!-- MASTER MACHINE -->
        <asp:Panel ID="pnlMaster" runat="server" Visible="false">

            <div class="card-box">
                <div style="font-weight:700;">Master Machine Upload</div>

                <asp:FileUpload ID="fuMasterFile" runat="server" />

                <asp:Button ID="btnSaveMasterFile"
                    runat="server"
                    Text="Save"
                    CssClass="btn-main"
                    OnClick="btnSaveMasterFile_Click" />
            </div>

            <div class="card-box">
                <asp:GridView ID="gvMasterFiles"
                    runat="server"
                    AutoGenerateColumns="false"
                    DataKeyNames="Id"
                    CssClass="grid-master"
                    OnRowCommand="gvMasterFiles_RowCommand">

                    <Columns>

                        <asp:BoundField DataField="FileName" HeaderText="File Name" />

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>

                                <a class="btn-view"
                                   href='<%# GetFileUrl(Eval("Id")) %>'
                                   target="_blank">
                                   View
                                </a>

                                <asp:LinkButton runat="server"
                                    CssClass="btn-delete"
                                    CommandName="deleteFile"
                                    CommandArgument="<%# Container.DataItemIndex %>"
                                    Text="Delete" />

                            </ItemTemplate>
                        </asp:TemplateField>

                    </Columns>

                </asp:GridView>
            </div>

        </asp:Panel>


        <!-- MACHINE FILE (Assist & Sewing) -->
        <asp:Panel ID="pnlMachine" runat="server" Visible="false">

            <!-- FORM -->
            <div class="card-box">

                <b>Upload Machine File</b><br /><br />

                <div class="form-group">
                    <label>Item:</label>

                    <div style="display:flex; gap:8px;">
                        <asp:TextBox ID="txtItem" runat="server" ReadOnly="true" CssClass="form-control" Width ="300" />

                        <asp:HiddenField ID="hfItemId" runat="server" />

                        <asp:Button ID="btnOpenPopup" runat="server" Text="Select Item" 
                            OnClick="btnOpenPopup_Click" CssClass="btn-main" />
                    </div>
                </div><br />

                <div class="form-group">
                    <label>Type:</label>

                    <div style="display:flex; gap:8px;">
                        <asp:TextBox ID="txtType" runat="server" ReadOnly="true" CssClass="form-control"  Width ="300"/>

                        <asp:HiddenField ID="hfTypeId" runat="server" />

                        <asp:Button ID="btnOpenType" runat="server" Text="Select Type" 
                            OnClick="btnOpenType_Click" CssClass="btn-main" />
                    </div>
                </div><br />

                <div class="form-group">
                    <label>File:</label>
                    <div style="display:flex; gap:8px;">
                        <asp:FileUpload ID="fuMachineFile" runat="server"  CssClass ="form-control mb-3" />
                    </div>
                </div><br />
    
                <asp:Button ID="btnUpload" runat="server" Text="Upload" CssClass="btn-main"
                    OnClick="btnUpload_Click" />

            </div>

            <!-- DATA -->
            <div class="card-box">
                <div style="font-weight:700; margin-bottom:10px;">Machine File List</div>

                <!-- SEARCH -->
                <div class="search-box">
                    <input type="text" id="searchInput" placeholder="Search..." onkeyup="searchGrid()" />
                </div>
                <asp:GridView ID="gvMachine" runat="server" 
                    AutoGenerateColumns="false"
                    CssClass="grid"
                    DataKeyNames="Id,TypeId"
                    OnRowCommand="gvMachine_RowCommand">

                    <Columns>
                        <asp:BoundField DataField="ItemName" HeaderText="Item" />
                        <asp:BoundField DataField="TypeName" HeaderText="Type" />
                        <asp:BoundField DataField="FileName" HeaderText="File" />

                        <asp:TemplateField HeaderText="View">
                            <ItemTemplate>
                                <a runat="server"
                                   href="javascript:void(0)"
                                   onclick='<%# string.Format("openPreview(\u0027{0}\u0027)", GetPreviewUrl(Eval("Id"))) %>'
                                   class="btn-action btn-view"
                                   Visible='<%# IsPreviewable(Eval("FileName")) %>'>
                                   Preview
                                </a>
                                <a href='<%# GetFileUrl(Eval("Id")) %>' target="_blank" class="btn-action btn-view">
                                   Download
                                </a>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>

                                <asp:LinkButton runat="server"
                                    CssClass="btn-action btn-edit"
                                    CommandName="editRow"
                                    CommandArgument="<%# Container.DataItemIndex %>"
                                    Text="Edit" />

                                <asp:LinkButton runat="server"
                                    CssClass="btn-action btn-delete"
                                    CommandName="deleteRow"
                                    CommandArgument="<%# Container.DataItemIndex %>"
                                    Text="Delete"
                                    OnClientClick="return confirm('Yakin mau delete?');" />

                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </asp:Panel>
        <div class="modal fade" id="previewModal" tabindex="-1">
          <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
              <div class="modal-header">
                <h5 class="modal-title">Preview</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
              </div>
              <div class="modal-body p-0">
                <iframe id="previewFrame"></iframe>
              </div>
            </div>
          </div>
        </div>
    </div>
    <!-- POPUP -->
    <asp:Panel ID="pnlItemPopup" runat="server" CssClass="modal-bg" Visible="false">
        <div class="modal-box">
            <h3>Item Management</h3>

            <asp:TextBox ID="txtItemNameModal" runat="server" placeholder="Item Name" />
            <asp:TextBox ID="txtSortModal" runat="server" placeholder="Sort Order" />

            <asp:Button ID="btnSaveItem" runat="server" Text="Save"
                OnClick="btnSaveItem_Click" CssClass="btn-main" />

            <div class="modal-scroll-area">
                <asp:GridView ID="gvItems" runat="server"
                    AutoGenerateColumns="false"
                    CssClass="grid"
                    DataKeyNames="Id"
                    OnRowCommand="gvItems_RowCommand">

                    <Columns>
                        <asp:BoundField DataField="ItemName" HeaderText="Item" />

                        <asp:BoundField DataField="SortOrder" HeaderText="Sort" />

                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>

                                <asp:LinkButton runat="server"
                                    CommandName="selectItem"
                                    CommandArgument="<%# Container.DataItemIndex %>"
                                    Text="Pilih" />

                                |

                                <asp:LinkButton runat="server"
                                    CommandName="editItem"
                                    CommandArgument="<%# Container.DataItemIndex %>"
                                    Text="Edit" />

                                |

                                <asp:LinkButton runat="server"
                                    CommandName="deleteItem"
                                    CommandArgument="<%# Container.DataItemIndex %>"
                                    Text="Delete" />

                            </ItemTemplate>
                        </asp:TemplateField>

                    </Columns>
                </asp:GridView>
            </div>

            <asp:Button ID="btnClosePopup" runat="server" Text="Close"
                OnClick="btnClosePopup_Click" />
        </div>
    </asp:Panel>

    <!-- POPUP TYPE -->
<asp:Panel ID="typeModal" runat="server" CssClass="modal-bg" Visible="false">
    <div class="modal-box">
        <div class="modal-header">
            <span>Type Management</span>
            <asp:Button ID="btnCloseType" runat="server" Text="X" CssClass="btn-close-modal" OnClick="btnCloseType_Click" />
        </div>

        <div style="margin-bottom:10px;">
            <asp:TextBox ID="txtTypeNameModal" runat="server" placeholder="Type Name" />
            <asp:TextBox ID="txtTypeSortModal" runat="server" placeholder="Sort Order" />
            <asp:Button ID="btnSaveType" runat="server" Text="Save" CssClass="btn-main" OnClick="btnSaveType_Click" />
        </div>

        <div class="modal-scroll-area">
            <asp:GridView ID="gvTypePopup" runat="server"
                AutoGenerateColumns="false"
                DataKeyNames="Id"
                OnRowCommand="gvTypePopup_RowCommand">

                <Columns>
                    <asp:BoundField DataField="TypeName" HeaderText="Type" />
                    <asp:BoundField DataField="SortOrder" HeaderText="Sort" />

                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:LinkButton runat="server"
                                CommandName="selectType"
                                CommandArgument="<%# Container.DataItemIndex %>"
                                Text="Pilih" />

                            |

                            <asp:LinkButton runat="server"
                                CommandName="editType"
                                CommandArgument="<%# Container.DataItemIndex %>"
                                Text="Edit" />

                            |

                            <asp:LinkButton runat="server"
                                CommandName="deleteType"
                                CommandArgument="<%# Container.DataItemIndex %>"
                                Text="Delete" />
                        </ItemTemplate>
                    </asp:TemplateField>

                </Columns>
             </asp:GridView>
        </div>
    </div>
</asp:Panel>
    <script>
        function searchGrid() {
            let input = document.getElementById("searchInput").value.toLowerCase();
            let rows = document.querySelectorAll("#<%= gvMachine.ClientID %> tr");

            rows.forEach((row, index) => {
                if (index === 0) return; // skip header

                let text = row.innerText.toLowerCase();

                row.style.display = text.includes(input) ? "" : "none";
            });
        }

        function openPreview(url) {
            var frame = document.getElementById("previewFrame");
            frame.src = url + "#toolbar=0&view=FitH";

            var modal = new bootstrap.Modal(document.getElementById('previewModal'));
            modal.show();
        }

        function resizePreview() {
            var modalBody = document.querySelector("#previewModal .modal-body");
            var frame = document.getElementById("previewFrame");

            if (modalBody && frame) {
                frame.style.height = modalBody.clientHeight + "px";
            }
        }

        window.addEventListener("resize", resizePreview);

        document.getElementById('previewModal').addEventListener('shown.bs.modal', resizePreview);

        document.getElementById('previewModal').addEventListener('hidden.bs.modal', function ()
        {
            document.getElementById("previewFrame").src = "";
        });

        function openItemPopup() {
            document.getElementById("itemModal").style.display = "flex";
            loadItems();
        }

        function closeItemPopup() {
            document.getElementById("itemModal").style.display = "none";
        }
    </script>
</asp:Content>