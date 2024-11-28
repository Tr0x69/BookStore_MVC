// From the instruction guide. Use AJAX to call data from API to table front end



//Jquery

var dataTable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/order/getall'  //Our api to get the data
    },
        columns: [
            { data: 'id', "width": "5%" },
            { data: 'applicationUser.name', "width": "20%" },
            { data: 'applicationUser.phoneNumber', "width": "20%" },
            { data: 'applicationUser.email', "width": "20%" },
            { data: 'orderTotal', "width": "10%" },
            { data: 'orderStatus', "width": "10%" },
            {
                data: 'id',
                "render": function (data) { //data is the id
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-fill"></i></a>
                    </div>`
                },
                "width": "10%"
            }
        ]



    });
}


