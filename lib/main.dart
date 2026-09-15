import 'package:flutter/material.dart';

void main() {
  runApp(  
    // Padding(
    //   padding: const EdgeInsets.only(
    //     top: 20.0,
    //     bottom: 20.0,
    //     left: 10.0,
    //     right: 10.0),
    //   child: Align(
    //     alignment: Alignment.topCenter,
    //     child:ConstrainedBox(
    //       constraints: const BoxConstraints(
    //         minWidth: 1.0,
    //         maxWidth: 50.0,
    //         minHeight: 100.0,
    //         maxHeight: 200.0,
    //       ),
    //       child: const Text(
    //         'Hello World',
    //         textDirection: TextDirection.ltr
    //         )
    //       )
    //    ),
    // )
    // Container(
    //   alignment: Alignment.topCenter,
    //   padding: EdgeInsets.symmetric(horizontal: 10, vertical: 20),
    //   margin: EdgeInsets.symmetric(vertical: 100,horizontal: 10),
    //   color: const Color.fromARGB(255, 9, 199, 199),
    //   width: 100,
    //   height: 200,
    //   child: const Text(
    //     'Hello World',
    //     textDirection: TextDirection.ltr,
    //   )
    // )
    // Column(
    //   mainAxisAlignment: MainAxisAlignment.spaceEvenly,
    //   crossAxisAlignment: CrossAxisAlignment.stretch,
    //   textDirection: TextDirection.ltr,
    //   verticalDirection: VerticalDirection.up,
    //   children: [
    //     Container(color: Colors.red,width: 100, height: 100),
    //     Container(color: Colors.yellow,width: 100, height: 100),
    //     Container(color: Colors.green,width: 100, height: 100)
        
    //   ],
    // )
    // Row(
    //   mainAxisAlignment: MainAxisAlignment.center,
    //   crossAxisAlignment: CrossAxisAlignment.center,
    //   textDirection: TextDirection.ltr,
    //   verticalDirection: VerticalDirection.up,
    //   children: [
        
    //     Expanded(child: Container (color: Colors.red,width: 100, height: 300)),
    //     Expanded(child: Container (color: Colors.yellow,width: 100, height: 300)),
    //     Expanded(child: Container (color: Colors.green,width: 100, height: 300)),
        
    //   ],

    // )




    Container(
      color: Colors.grey,
      padding: EdgeInsets.all(20),
      child: Column(
        textDirection: TextDirection.ltr,
        children: [
          Container(
            margin: EdgeInsets.symmetric(vertical: 30,horizontal: 10),
            height: 290,
            child: Column(
              textDirection: TextDirection.ltr,
              children: [
                Expanded(child: Container(color: Colors.white)),
                Expanded(child: Container(color: Colors.blue)),
                Expanded(child: Container(color: Colors.red)),
              ],
            ),
          ),
          Container(
            margin: EdgeInsets.symmetric(vertical: 30,horizontal: 10),
            height: 290,
            child: Row(
              textDirection: TextDirection.ltr,
              children: [
                Expanded(child: Container(color: Colors.blue)),
                Expanded(child: Container(color: Colors.yellow)),
                Expanded(child: Container(color: Colors.red)),
              ],
            ),
          ),
        ],
      ),
    ),
  );
}

  

