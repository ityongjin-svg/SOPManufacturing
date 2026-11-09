<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SOPManagement.aspx.cs" Inherits="SOP_UI.Main.SOPManagement" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .form-card {
            background: #fff;
            padding: 20px;
            border-radius: 12px;
            box-shadow: 0 5px 20px rgba(0,0,0,0.08);
            transition: 0.3s;
        }

        .form-card:hover {
            transform: translateY(-3px);
            box-shadow: 0 10px 30px rgba(0,0,0,0.12);
        }

        .nav-tabs .nav-link {
            border-radius: 20px;
            margin-right: 5px;
            font-weight: 500;
        }

        .nav-tabs .nav-link.active {
            background: #0d6efd;
            color: white;
        }

        img:hover {
            transform: scale(1.05);
            transition: 0.2s;
        }

        .section-thumb {
            width: 60px;
            height: 60px;
            object-fit: cover;
            border-radius: 8px;
            cursor: pointer;
            border: 1px solid #e2e8f0;
        }

        .section-thumb:hover {
            transform: scale(1.05);
            transition: 0.2s;
        }
    </style>

    <div class="container mt-4">

        <h3 class="mb-4">⚙️ SOP Management</h3>

        <!-- TAB NAV -->
        <ul class="nav nav-tabs mb-3">
            <li class="nav-item">
                <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#sectionTab">Section</button>
            </li>
            <li class="nav-item">
                <button class="nav-link" data-bs-toggle="tab" data-bs-target="#groupTab">Group</button>
            </li>
            <li class="nav-item">
                <button class="nav-link" data-bs-toggle="tab" data-bs-target="#itemTab">SOP Item</button>
            </li>
        </ul>

        <div class="tab-content">

            <!-- ================= SECTION ================= -->
            <div class="tab-pane fade show active" id="sectionTab">

                <div class="form-card">

                    <h5 class="mb-3">📁 Section Management</h5>

                    <asp:DropDownList ID="ddlMenuSection" runat="server"
                        CssClass="form-control mb-3" />

                    <asp:TextBox ID="txtSectionName" runat="server"
                        CssClass="form-control mb-3"
                        placeholder="Section Name..." />

                    <asp:FileUpload ID="fuSectionImage" runat="server"
                        CssClass="form-control mb-2" />

                    <div id="pnlCurrentImage" runat="server" visible="false" class="mb-3">
                        <small class="text-muted d-block mb-1">Current image (kosongkan upload jika tidak ingin mengganti):</small>
                        <asp:Image ID="imgCurrentSection" runat="server" CssClass="section-thumb" />
                    </div>

                    <asp:TextBox ID="txtSort" runat="server"
                        CssClass="form-control mb-3"
                        placeholder="Urutan Ke..." />

                    <asp:Button ID="btnSaveSection" runat="server"
                        Text="Save Section"
                        CssClass="btn btn-primary w-100"
                        OnClick="btnSaveSection_Click" />

                    <hr class="mt-4" />

                    <h6 class="mb-2">📋 Section List</h6>

                    <asp:GridView ID="gvSection" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-hover table-striped mt-2"
                        DataKeyNames="Id"
                        OnRowCommand="gvSection_RowCommand">

                        <Columns>
                            <asp:BoundField DataField="SortOrder" HeaderText="Urutan" />
                            <asp:BoundField DataField="Name" HeaderText="Section Name" />

                            <asp:TemplateField HeaderText="Image">
                                <ItemTemplate>
                                    <img src='<%# GetImageUrl(Eval("Id")) %>'
                                            class="section-thumb"
                                            onerror="this.src='/images/no-image.png'"
                                            onclick="showImage(this.src)" />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:ButtonField Text="Edit" CommandName="editBaris" />
                            <asp:ButtonField Text="Delete" CommandName="deleteBaris" />
                        </Columns>

                    </asp:GridView>
                </div>
            </div>

            <!-- ================= GROUP ================= -->
            <div class="tab-pane fade" id="groupTab">

                <div class="form-card">

                    <h5 class="mb-3">📂 Group Management</h5>

                    <asp:DropDownList ID="ddlMenuGroup" runat="server"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ddlMenuGroup_Changed"
                        CssClass="form-control mb-3" />

                    <asp:DropDownList ID="ddlSectionGroup" runat="server"
                        CssClass="form-control mb-3" />

                    <asp:TextBox ID="txtGroupName" runat="server"
                        CssClass="form-control mb-3"
                        placeholder="Group Name..." />

                    <asp:Button ID="btnSaveGroup" runat="server"
                        Text="Save Group"
                        CssClass="btn btn-primary w-100"
                        OnClick="btnSaveGroup_Click" />

                    <hr class="mt-4" />

                    <h6 class="mb-2">📋 Section List</h6>

                    <asp:GridView ID="gvGroup" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-hover table-striped"
                        DataKeyNames="Id"
                        OnRowCommand="gvGroup_RowCommand">

                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Group" />
                            <asp:BoundField DataField="SectionName" HeaderText="Section" />

                            <asp:ButtonField Text="Edit" CommandName="editBaris" />
                            <asp:ButtonField Text="Delete" CommandName="deleteBaris" />
                        </Columns>

                    </asp:GridView>
                </div>
            </div>

            <!-- ================= SOP ITEM ================= -->
            <div class="tab-pane fade" id="itemTab">

                <div class="form-card">

                    <h5 class="mb-3">📄 SOP Item Management</h5>

                    <asp:DropDownList ID="ddlMenuItem" runat="server"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ddlMenuItem_Changed"
                        CssClass="form-control mb-3" />

                    <asp:DropDownList ID="ddlSectionItem" runat="server"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ddlSectionItem_Changed"
                        CssClass="form-control mb-3" />

                    <asp:DropDownList ID="ddlGroupItem" runat="server"
                        CssClass="form-control mb-3" />

                    <asp:DropDownList ID="ddlCategory" runat="server"
                        CssClass="form-control mb-3" />

                    <asp:TextBox ID="txtItemTitle" runat="server"
                        CssClass="form-control mb-3"
                        placeholder="Item Title (A-1 Work Instruction...)" />

                    <asp:FileUpload ID="fuSOPFile" runat="server"
                        CssClass="form-control mb-3" />

                    <asp:Button ID="btnSaveItem" runat="server"
                        Text="Upload SOP"
                        CssClass="btn btn-success w-100"
                        OnClick="btnSaveItem_Click" />

                </div>

                <asp:GridView ID="gvItem" runat="server"
                    AutoGenerateColumns="false"
                    CssClass="table table-hover table-striped"
                    DataKeyNames="Id"
                    OnRowCommand="gvItem_RowCommand">

                    <Columns>

                        <asp:BoundField DataField="ItemTitle" HeaderText="Title" />
                        <asp:BoundField DataField="GroupName" HeaderText="Group" />
                        <asp:BoundField DataField="SectionName" HeaderText="Section" />

                        <asp:TemplateField HeaderText="File">
                            <ItemTemplate>
                                <a href='<%# Eval("FilePath") %>' target="_blank" class="btn btn-sm btn-outline-primary">
                                    Download
                                </a>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:ButtonField Text="Edit"
                            CommandName="editBaris"
                            ButtonType="Button" />

                        <asp:ButtonField Text="Delete"
                            CommandName="deleteBaris"
                            ButtonType="Button" />

                    </Columns>

                </asp:GridView>
            </div>
        </div>
    </div>

    <div class="modal fade" id="imageModal" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">

                <div class="modal-header">
                    <h5 class="modal-title">Image Preview</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>

                <div class="modal-body text-center">
                    <img id="previewImage" src="" style="max-width:100%;border-radius:10px;" />
                </div>

            </div>
        </div>
    </div>
    <script>
        document.addEventListener("DOMContentLoaded", function () {

            // 🔹 Restore tab terakhir
            let activeTab = localStorage.getItem("activeTab");
            if (activeTab) {
                let triggerEl = document.querySelector(`[data-bs-target="${activeTab}"]`);
                if (triggerEl) {
                    new bootstrap.Tab(triggerEl).show();
                }
            }

            // 🔹 Save tab saat diklik
            let tabs = document.querySelectorAll('[data-bs-toggle="tab"]');

            tabs.forEach(tab => {
                tab.addEventListener("shown.bs.tab", function (e) {
                    let target = e.target.getAttribute("data-bs-target");
                    localStorage.setItem("activeTab", target);
                });
            });

        });
        function showImage(src) {
            document.getElementById("previewImage").src = src;

            var myModal = new bootstrap.Modal(document.getElementById('imageModal'));
            myModal.show();
        }
    </script>
</asp:Content>