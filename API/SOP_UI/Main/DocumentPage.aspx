<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DocumentPage.aspx.cs" Inherits="SOP_UI.Main.DocumentPage" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
    /* ================= BACKGROUND ================= */
    .page-wrapper {
        min-height: 100vh;
        background: radial-gradient(circle at top left, #e0c89f, #e0ded7 100%);
        padding: 40px 0;
    }

    /* ================= HEADER ================= */
    .header-box {
        background: white;
        border-radius: 14px;
        padding: 18px;
        box-shadow: 0 8px 25px rgb(0 0 0 / 50%);
        margin-bottom: 25px;
    }

    .page-title {
        font-size: 22px;
        font-weight: 700;
    }

    .search-box {
        width: 250px;
    }

    /* ================= TABS ================= */
    .tabs {
        margin-bottom: 25px;
    }

    .tab {
        padding: 8px 16px;
        border-radius: 50px;
        background: #e9ecef;
        text-decoration: none;
        margin-right: 8px;
        color: #333;
        font-size: 13px;
        transition: 0.2s;
    }

    .tab:hover {
        background: #0d6efd;
        color: white;
    }

    .active-tab {
        background: #0d6efd;
        color: white;
    }

    /* ================= CARD ================= */
    .doc-card {
        border-radius: 18px;
        overflow: hidden;
        background: white;
        box-shadow: 0 10px 25px rgba(0 0 0 / 50%);
        transition: all 0.3s ease;
        position: relative;
    }

    .doc-card:hover {
        transform: translateY(-6px);
        box-shadow: 0 18px 40px rgba(0 0 0 / 80%);
    }

    /* ================= THUMB ================= */
    .doc-thumb {
        height: 180px;
        overflow: hidden;
        position: relative;
        left:-2px;
        top:-2px;
    }

    .doc-thumb iframe {
        width: 110%;
        height: 115%;
        transform: scale(1);
        transform-origin: top left;
        border: none;
        pointer-events: none;
    }


    /* ================= FLOAT BUTTON ================= */
    .arrow-btn {
        position: absolute;
        bottom: 12px;
        right: 12px;
        width: 42px;
        height: 42px;
        border-radius: 50%;
        background: linear-gradient(135deg, #c89c7c, #b5835a);
        color: white;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 18px;
        cursor: pointer;
        transition: all 0.25s ease;
    }

    .arrow-btn:hover {
        transform: scale(1.2);
        background: #b5835a;
    }

    /* ================= BODY ================= */
    .doc-body {
        padding: 14px 16px;
    }

    .doc-title {
        font-size: 15px;
        font-weight: 600;
        height: 40px;
        overflow: hidden;
    }

    .download-btn {
        margin-top: 8px;
    }

    /* ================= ANIMATION ================= */
    .fade-in {
        animation: fadeUp 0.4s ease;
    }

    @keyframes fadeUp {
        from {
            opacity: 0;
            transform: translateY(12px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
    /* =============== Preview ================*/
    .modal-dialog {
        width : 90%;
        max-width: 1200px;
    }

    .modal-content {
        max-height: 700px;
        height: 50%;
    }

    .modal-body {
        height: calc(90vh - 60px); /* header height */
        padding: 0;
    }

    #previewFrame {
        width: 100%;
        height: 100%;
        border: none;
    }

    .doc-icon-fallback {
        height: 180px;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #f8fafc;
    }

    .doc-icon-fallback img {
        width: 64px;
        height: 64px;
        object-fit: contain;
        opacity: 0.85;
    }
    </style>
    <asp:HiddenField ID="hfHasAccess" runat="server" />
    <div class="page-wrapper">

        <div class="container">

            <!-- HEADER -->
            <div class="header-box d-flex justify-content-between align-items-center">

                <asp:Label ID="lblTitle" runat="server" CssClass="page-title"></asp:Label>

                <input type="text" id="searchBox" class="form-control search-box" placeholder="Search document..." />

            </div>

            <!-- TABS -->
            <div id="tabContainer" runat="server"></div>

            <!-- GRID -->
            <div class="row" id="docContainer">
                <asp:Repeater ID="rptDocs" runat="server">
                    <ItemTemplate>
                        <div class="col-lg-3 col-md-4 col-sm-6 mb-4 doc-item">
                            <div class="doc-card fade-in">
                                <!-- THUMB -->
                                <div class="doc-thumb">
                                    <%# GetThumbHtml(Eval("previewPath"), Eval("fileType")) %>
                                </div>

                                <!-- BODY -->
                                <div class="doc-body">

                                    <div class="doc-title">
                                        <%# Eval("title") %>
                                    </div>

                                    <a href="javascript:void(0)"
                                       onclick='<%# GetDownloadOnClick(Eval("filePath")) %>'
                                       class="btn btn-outline-primary btn-sm download-btn w-100">
                                        Download
                                    </a>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

        </div>

    </div>

    <!-- MODAL -->
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

    <div class="modal fade" id="noAccessModal" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered modal-sm">
        <div class="modal-content" style="border-radius:16px; text-align:center; padding:20px;">
          <div style="font-size:40px; color:#ef4444;">🚫</div>
          <h5 style="margin-top:10px;">Access Denied</h5>
          <p style="color:#64748b; font-size:14px;">
            You do not have permission to download this file.<br/>
            Contact admin to get access.
          </p>
          <button type="button" class="btn btn-main" data-bs-dismiss="modal" style="border-radius:20px;">
            OK
          </button>
        </div>
      </div>
    </div>

    <script>
        document.addEventListener("DOMContentLoaded", function () {

            // SEARCH
            const searchBox = document.getElementById("searchBox");

            if (searchBox) {
                searchBox.addEventListener("keyup", function () {
                    let value = this.value.toLowerCase();
                    let items = document.querySelectorAll(".doc-item");

                    items.forEach(i => {
                        let text = i.innerText.toLowerCase();
                        i.style.display = text.includes(value) ? "" : "none";
                    });
                });
            }

        });

        // PREVIEW
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

        document.getElementById('previewModal')
            .addEventListener('shown.bs.modal', resizePreview);


        // CLEAR FRAME
        document.getElementById('previewModal')
            .addEventListener('hidden.bs.modal', function () {
                document.getElementById("previewFrame").src = "";
            });

        function isDownloadAllowed() {
            return document.getElementById('<%= hfHasAccess.ClientID %>').value === "true";
        }

        function showNoAccess() {
            var modal = new bootstrap.Modal(document.getElementById('noAccessModal'));
            modal.show();
        }

        function handleDownload(url) {
            if (!isDownloadAllowed()) {
                showNoAccess();
                return;
            }
            window.open(url, '_blank');
        }
    </script>

</asp:Content>