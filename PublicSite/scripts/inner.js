function toggleIcon(el)
{
  if($(el).hasClass("collapsed") == true)
  {
      removeIconClasses(el);

      el.querySelector(".expandIcon").classList.add("fa-minus");
  }
  else
  {
      removeIconClasses(el);

      el.querySelector(".expandIcon").classList.add("fa-plus");
  }
}

function removeIconClasses(el)
{
  $(el).find(".expandIcon").removeClass("fa-minus");
  $(el).find(".expandIcon").removeClass("fa-plus");
}