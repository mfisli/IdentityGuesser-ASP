function log(msg){
    console.log(msg);
}
window.onload = function () {
    log("Loaded custom js script");
    var trigger = document.getElementsByClassName("uploadBtn")[0];
    trigger.addEventListener("click", function(){
        log("Trigger fired.");
        var loading = document.getElementById("loading");
        log(loading);
        if (loading) {
            log(loading);
            var source = "https://upload.wikimedia.org/wikipedia/commons/b/b1/Loading_icon.gif"
            loading.innerHTML = "<img src=\'" + source + "\' alt='loading...'/>";
        }   
    });
}