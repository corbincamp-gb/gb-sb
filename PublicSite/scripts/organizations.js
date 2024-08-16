var table;

$(document).ready(function(){
    SetupDataTable();
});

function SetupDataTable()
{
    if($('#org-table').length > 0)
	{
		table = $('#org-table').DataTable({
            "data":orgs.data,
			responsive: {
                details: {
                  type: 'column',
                  target: 0,
                },
              },
              autoWidth: false,
              bAutoWidth: false,
              dom: "<'row'<'col-sm-12 col-md-6'l<'#table-reset'><'#table-legend'>><'col-sm-12 col-md-6'f>>" +
              "<'row'<'col-sm-12'tr>>" +
              "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
              order: [[1, 'asc']],
              "columnDefs": [
                { "width": "10%", "targets": 0, "className": "control", "sortable": false },
                { "width": "60%", "targets": 1 },
                { "width": "20%", "targets": 2 },
                { "width": "20%", "targets": 3 }
              ],
              lengthMenu: [10, 25, 50],
              pageLength: 10,
            "columns": [
            {
                "render": function ( data, type, row ) {
                    return "";
                }
            },
            {
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
                "data": "OPPORTUNITY_TYPE"                
            }, {
                "data": "DELIVERY_METHOD"                
            }, {
                "data": "PROGRAM_DURATION"                
            }, {
                "data": "STATES"                
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
                "data": "COHORTS"                
            }, {
                "data": "JOB_FAMILY"                
            }, {
                //"data": "LOCATION_DETAILS_AVAILABLE"  
                "render": function ( data, type, row ) {
                    //return "<a href='military-members.htm?branch=" + row.SERVICE +  "&base=" + row.INSTALLATION +  "&program=" + row.PROGRAM + "' class='btn-primary btn apply-btn' style='color:#ffffff;text-decoration:none !important;' data-service='" + row.SERVICE +  "' data-installation='" + row.INSTALLATION +  "' data-program='" + row.PROGRAM + "'>Apply</a>";
                    if(row.LOCATION_DETAILS_AVAILABLE != "")
                    {
                        if(row.LOCATION_DETAILS_AVAILABLE == 1)
                        {
                            return "<a href='locations.htm' title='View Locations Page'>Yes</a>";
                        }
                        else
                        {
                            return "No";
                        }
                    }
                    else
                    {
                        return "No";
                    }                    
                }              
            } 
            ],
			"destroy": true
		});

        $.fn.DataTable.ext.pager.numbers_length = 5;
	}
}