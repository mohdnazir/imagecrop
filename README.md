# imagecrop
Crop and upload images from html page using asp.net, jCrop and jQuery. It uses an open source jQuery based library JCrop. 

## How it works? ##
A file upload control is used to browse and upload image on web page. Immidiatly image is enacoded into base64 format. Rather than uploading 
on server, images is display on client machine only. Once image uploaded, JCrop is initialized, which facilitate to user to 
choose the section of image which needs to be crroped. On the click of crop button, image selection coordinates and full image encoded in 
base64 is uploaded on server. Using windows drawing library, image is cropped and sent back to user.

## Code ##
**Java Script**
```Javascript
        jQuery(function ($) {
            var jcrop_api;
            function intiCrop() {
                $('#target').Jcrop({
                    onSelect: onSelect,
                    onChange: onChange,
                    bgColor: 'gray',
                    bgOpacity: .6,
                    setSelect: [0, 0, 100, 100],
                    aspectRatio: 0
                }, function () {
                    jcrop_api = this;
                });
            }
            intiCrop();
            function onSelect(c) {
                console.log("Selected x:" + c.x + " y:" + c.y + " x2:" + c.x2 + " y2:" + c.y2 + " w:" + c.w + " h:" + c.h);
                $("#x").val(c.x);
                $("#y").val(c.y);
                $("#h").val(c.h);
                $("#w").val(c.w);
            }

            function onChange(c) {
                console.log("Movedx:" + c.x + " y:" + c.y + " x2:" + c.x2 + " y2:" + c.y2 + " w:" + c.w + " h:" + c.h);
                $("#x").val(c.x);
                $("#y").val(c.y);
                $("#h").val(c.h);
                $("#w").val(c.w);
            }
            $("#myfile").change(function () {
                if (jcrop_api != undefined)
                    jcrop_api.destroy();
                encodeImagetoBase64(this);
            });

            $("#crop").click(function () {
                //alert("Selected x:" + $("#x").val() + " y:" + $("#y").val() + " w:" + $("#w").val() + " h:" + $("#h").val());
                $.ajax({
                    type: "POST",
                    url: "/Home/Crop",
                    data: { "myimg": $("#imgbase64").val(), "x": $("#x").val(), "y": $("#y").val(), "height": $("#h").val(), "width": $("#w").val() },
                    dataType: "json",
                    success: function (data) {
                        if (jcrop_api != undefined)
                            jcrop_api.destroy();
                        $("#target").attr("src", data)
                        $("#target").height($("#h").val());
                        $("#target").width($("#w").val());
                        //alert(data);
                    },
                    error: function () {
                        alert("Error occured!!")
                    }
                });
            })

            function encodeImagetoBase64(element) {
                var file = element.files[0]; 
                var reader = new FileReader();
                reader.onloadend = function () {
                    $("#imgbase64").val(reader.result);
                    $("#target").attr("src", reader.result);
                    //document.getElementById('target').setAttribute('src', reader.result);
                    intiCrop();
                }
                reader.readAsDataURL(file);
            }
        });
```

**C#**
```C#
        [HttpPost]
        public ActionResult Crop(string myimg, int x, int y, int height, int width)
        {
            string basestring = myimg.Substring(0, myimg.LastIndexOf("base64,") + 7);
            byte[] image = Convert.FromBase64String(myimg.Replace(basestring, ""));
            string imgpath = Path.GetTempFileName();
            System.IO.File.WriteAllBytes(imgpath, image);

            Rectangle cropRect = new Rectangle(new Point(x, y), new Size(width, height));
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Bitmap src = Image.FromFile(imgpath) as Bitmap)
            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            System.IO.File.Delete(imgpath);
            MemoryStream ms = new MemoryStream();
            target.Save(ms, GetImageFormat(basestring));
            byte[] byteImage = ms.ToArray();
            var SigBase64 = Convert.ToBase64String(byteImage); //Get Base64
            return Json(basestring + SigBase64, JsonRequestBehavior.AllowGet);
        }
        ImageFormat GetImageFormat(string base64ref)
        {
            if (base64ref.Contains("/png"))
                return ImageFormat.Png;
            if (base64ref.Contains("/gif"))
                return ImageFormat.Gif;
            if (base64ref.Contains("/jpg"))
                return ImageFormat.Jpeg;

            return ImageFormat.Jpeg;
        }
```

## Supported Image Format ##
1. png
1. jpg (jpeg)
1. gif
