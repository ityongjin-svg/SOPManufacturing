<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SOP_UI._Default" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .page-header .filter::after {
            background: linear-gradient(
                rgba(0,0,0,0.7),
                rgba(150, 186, 217,0.3)
            );
            content: "";
            display: block;
            height: 100%;
            left: 0;
            top: 0;
            position: absolute;
            width: 100%;
            z-index: 1;
        }

        .page-header.page-header-small {
            min-height: 65vh !important;
            max-height: 700px;
        }

        .page-header.page-header-xs {
            min-height: 40vh !important;
        }

        .page-header.section-dark .content-center,
        .page-header.section-dark .category-absolute {
            z-index: 1;
        }

        .page-header {
            background-color: #B2AFAB;
            background-position: center center;
            background-size: cover;
            min-height: 100vh;
            max-height: 999px;
            overflow: hidden;
            position: relative;
            width: 100%;
            z-index: 1;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .page-header .content-center {
            margin-top: 85px;
        }

        .moving-clouds {
            position: absolute;
            z-index: 1;
            bottom: 0;
            left: 0;
            width: 250.625em;
            height: 43.75em;
            -webkit-animation: cloudLoop 80s linear infinite;
            animation: cloudLoop 80s linear infinite;
        }

        @keyframes cloudLoop {
            0% {
                -webkit-transform: translate3d(0, 0, 0);
                transform: translate3d(0, 0, 0);
            }

            100% {
                -webkit-transform: translate3d(-50%, 0, 0);
                transform: translate3d(-50%, 0, 0);
            }
        }

        .title-brand{
            max-width: 730px;
            margin: 0 auto;
            position: relative;
            text-align: center;
            color: #FFFFFF !important;
            display: block;
        }
        .title-brand h1 {
            font-size: 48px;
            font-weight: 700;
            letter-spacing: 1px;
            color:#fff !important;
        }

        .title-brand span {
            font-size: 16px;
            font-weight: 800;
            opacity: 0.9;
        }

        .title-brand {
            animation: fadeUp 1s ease;
        }

        @keyframes fadeUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .fog-low {
            margin-bottom: -35px;
        }

        .fog-low {
            position: absolute;
            left: 0;
            bottom: 0;
            margin-left: -35%;
            margin-bottom: -50px;
            width: 110%;
            opacity: .85;
            top: 30px;
        }

        .fog-low img {
            width: 100%;
        }

        .fog-low.right {
            margin-left: 30%;
            opacity: 1;
        }

        @media (max-width: 768px) {
            .title-brand h1 {
                font-size: 28px;
            }

            .title-brand span {
                font-size: 14px;
            }

            .fog-low {
                display: none; /* biar tidak berantakan di mobile */
            }
        }

    </style>
    <div>
        <div class="page-header section-dark" style="background-image: url('<%= ResolveUrl("~/assets/img/background-Home.jpg") %>')">
            <div class="filter"></div>
            <div class="content-center">
                <div class="container">
                    <div class="title-brand">
                        <h1>Garment Manufacturing SOP</h1>
                        <br>
                        <span>부서별 업무 프로세스 및 품질 관리 절차</span>
                        <div class="fog-low">
                            <img src="http://demos.creative-tim.com/paper-kit-2/assets/img/fog-low.png" alt="">
                        </div>
                        <div class="fog-low right">
                            <img src="http://demos.creative-tim.com/paper-kit-2/assets/img/fog-low.png" alt="">
                        </div>
                    </div>

                    <h2 class="presentation-subtitle text-center"></h2>
                </div>
            </div>
            <div class="moving-clouds" style="background-image: url('http://demos.creative-tim.com/paper-kit-2/assets/img/clouds.png'); ">

            </div>
        </div>
    </div>

</asp:Content>