var dataTable;

$(document).ready(function () {
    loadDataTable();
});

//https://datatables.net/
function loadDataTable() {
    //tblData id iz product index.cshtml
    dataTable = $("#tblData").DataTable({
        "ajax": {
            url: '/admin/product/getall'
        },
        "columns": [
            { data: 'title', "width": "25%" },
            { data: 'isbn', "width": "20%" },
            { data: 'author', "width": "25%" },
            { data: 'listPrice', "width": "10%" },
            { data: 'category.name', "width": "20%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                        <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2"><i class="bi bu-pencil-square"></i>Edit</a>
                        <a onClick=Delete('/admin/product/delete/${data}') class="btn btn-primary mx-2"><i class="bi bu-pencil-square"></i>Delete</a>
                    </div>`
                },
                "width": "25%"
            },
        ]
    })
}

//sweetAlerts2
function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                }
            })
        }
    });
}