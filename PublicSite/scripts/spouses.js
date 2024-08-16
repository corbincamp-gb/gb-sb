var table;

$(document).ready(function(){
    SetupDataTable();

    $('#collapseOne').on('shown.bs.collapse', function () {
        table.responsive.recalc()
    })
    
});

function SetupDataTable()
{
    if($('#spouses-table').length > 0)
	{
		table = $('#spouses-table').DataTable({
            "data":spouses.data,
			responsive: true,
			"autoWidth":false,
			"bAutoWidth":false,
            "dom":"<'row'<'col-sm-12 col-md-6'l<'#table-reset'><'#table-legend'>><'col-sm-12 col-md-6'f>>" +
                    "<'row'<'col-sm-12'tr>>" +
                    "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
            "order": [
                [0, 'asc']
            ],
            "columnDefs": [
                { "width": "45%", "targets": 0 },
                { "width": "15%", "targets": 1 },
                { "width": "10%", "targets": 2 },
                { "width": "15%", "targets": 3 },
                { "width": "15%", "targets": 4 }
              ],
            "columns": [
            {
                /*
                <th>Program</th>
                <th>Nationwide Locations</th>
                <th>Online</th>
                <th>Delivery Method</th>
                <th>State(s) of Program Delivery</th>
                */
               /*
                "PROGRAM": "DefendEdge OC, LLC",
                "URL": "https://www.defendedge.com/solutions/managed-security-provider/",
                "NATIONWIDE": 0,
                "ONLINE": 0,
                "DELIVERY_METHOD": "Hybrid (in-person and online)",
                "STATES": "DC, FL, IL"
               */
                //"data": "SERVICE",PROGRAM URL
                "render": function ( data, type, row ) {
                    //return "<a href='military-members.htm?branch=" + row.SERVICE +  "&base=" + row.INSTALLATION +  "&program=" + row.PROGRAM + "' class='btn-primary btn apply-btn' style='color:#ffffff;text-decoration:none !important;' data-service='" + row.SERVICE +  "' data-installation='" + row.INSTALLATION +  "' data-program='" + row.PROGRAM + "'>Apply</a>";
                    if(row.URL != "")
                    {
                        return "<a href='" + row.URL + "' target='_blank' title='External Link to: " + row.PROGRAM + "'>" + row.PROGRAM + "</a>";
                    }
                    else
                    {
                        return row.PROGRAM;
                    }
                }
            }, {
                //"data": "NATIONWIDE"
                "render": function ( data, type, row ) {
                    //return "<a href='military-members.htm?branch=" + row.SERVICE +  "&base=" + row.INSTALLATION +  "&program=" + row.PROGRAM + "' class='btn-primary btn apply-btn' style='color:#ffffff;text-decoration:none !important;' data-service='" + row.SERVICE +  "' data-installation='" + row.INSTALLATION +  "' data-program='" + row.PROGRAM + "'>Apply</a>";
                    if(row.NATIONWIDE != "")
                    {
                        if(row.NATIONWIDE == 1)
                        {
                            return "<span class='org-list-icon'>&#x2020;</span><span class='table-hidden-data'>Nationwide</span>";
                        }
                        else
                        {
                            return "";
                        }  
                    }
                    else
                    {
                        return "";
                    }                    
                }
            }, {
                //"data": "ONLINE"
                "render": function ( data, type, row ) {
                    //return "<a href='military-members.htm?branch=" + row.SERVICE +  "&base=" + row.INSTALLATION +  "&program=" + row.PROGRAM + "' class='btn-primary btn apply-btn' style='color:#ffffff;text-decoration:none !important;' data-service='" + row.SERVICE +  "' data-installation='" + row.INSTALLATION +  "' data-program='" + row.PROGRAM + "'>Apply</a>";
                    if(row.ONLINE != "")
                    {
                        if(row.ONLINE == 1)
                        {
                            return "<span class='org-list-icon'>&#42;</span><span class='table-hidden-data'>Online</span>";
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else
                    {
                        return "";
                    }                    
                }
            }, {
                "data": "DELIVERY_METHOD"                
            }, {
                "data": "STATES"                
            }
            ],
			"destroy": true,
            "lengthMenu": [ 10, 25, 50 ],
            "pageLength": 10
		});
	}
}