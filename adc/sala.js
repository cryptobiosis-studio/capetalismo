export default class sala extends Phaser.Scene {
  constructor (){
    super('sala')
  }

  preload (){
    this.load.image('sala', 'assets/sala.png')
  }

  create (){
    this.add.image(400, 225, 'sala')
    .setInteractive()
    .on('pointerdown', () =>{
      this.scene.stop('abertura')
      this.scene.start('sala')
    })
  }
}