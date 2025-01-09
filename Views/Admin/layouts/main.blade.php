<!DOCTYPE html>
<html lang="en">

<head>
    @await Html.PartialAsync("admin/includes/head")
</head>

<body>
    @await Html.PartialAsync("admin/includes/header")

    @RenderBody()
    @RenderSection("content", required: false)

    @await Html.PartialAsync("admin/includes/js_lib")
</body>

</html>
